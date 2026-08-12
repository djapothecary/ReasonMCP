using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;

namespace ReasonMCP.Workflows
{
    public class DocumentWorkflow
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DocumentScanSettings _settings;
        private readonly StorageConfigSettings _storageSettings;
        private readonly ILogger<DocumentWorkflow> _logger;

        public DocumentWorkflow(
            IServiceScopeFactory scopeFactory,
            IOptions<DocumentScanSettings> options,
            IOptions<StorageConfigSettings> storageOptions,
            ILogger<DocumentWorkflow> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _storageSettings = storageOptions.Value;
            _logger = logger;
        }

        public async Task RunAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (!_settings.Enabled)
                return;

            //  1.  Call the service to scan directories
            //  this will also perform upsert to ingestion queue
            if (_settings.RunFileScan)
            {
                var scope = _scopeFactory.CreateScope();
                var documentScanService = scope
                    .ServiceProvider
                    .GetRequiredService<IDocumentScanService>();

                _logger.LogInformation("Starting Document Scan ...");
                await documentScanService.ScanDocumentsAsync(
                    cancellationToken
                );

                //  intentionally Disposing and creating scope
                //  so that scope is managed in configurable blocks
                scope.Dispose();
            }

            //  2.  Dequeue next record
            if (_settings.ProcessFiles)
            {
                var scope = _scopeFactory.CreateScope();
                var ingestionQueue = scope
                    .ServiceProvider
                    .GetRequiredService<IIngestionQueueService>();

                var documentProcessor = scope
                    .ServiceProvider
                    .GetRequiredService<IDocumentsProcessor>();

                var strategies = scope
                    .ServiceProvider
                    .GetRequiredService<IEnumerable<IFileConverterStrategy>>();

                var fileConverter = scope
                    .ServiceProvider
                    .GetRequiredService<IFileConverterUtility>();

                int filesprocessed = 0;
                int filesToProcess = await ingestionQueue.GetCountIngestedRecordsAsync(
                    "Documents",
                    cancellationToken
                );

                while (filesprocessed < filesToProcess)
                {
                    var fileIngestionRecord = await documentProcessor
                        .GetNextDocumentFileAsync(
                            cancellationToken
                        );

                    var filePath = fileIngestionRecord.FilePath;

                    try
                    {
                        //  get the strategies to enrich the Document record

                        //  Determine file type and what processor to use
                        var strategy = strategies.FirstOrDefault(
                            s => s.CanConvert(
                                filePath
                            )
                        );

                        bool convertSuccess;
                        bool writeConvertedOutput = _settings.WriteConvertedOutput;

                        //  perform Enrichment
                        convertSuccess = await strategy!
                            .ConvertForIngestionAsync(
                                filePath,
                                writeConvertedOutput,
                                cancellationToken
                            );

                        //  update status
                        if (convertSuccess)
                        {
                            await ingestionQueue.MarkConversionCompleteAsync(
                                    filePath,
                                    cancellationToken
                            );
                        }
                        else
                        {
                            await ingestionQueue.MarkConversionFailedAsync(
                                filePath,
                                "Document record conversion failed",
                                cancellationToken
                            );
                        }

                        //  Clear orginal filees (or not)
                        if (_storageSettings.ClearOriginalFile)
                        {
                            await fileConverter.ClearOriginalFile(filePath);
                        }

                        Console.WriteLine(filePath);
                        filesprocessed++;
                    }
                    catch (Exception whileEx)
                    {
                        _logger.LogError(
                            $"An error occurred while performing Document ingestion: {whileEx.Message}",
                            whileEx
                        );

                        await ingestionQueue.MarkFailedExceptionAsync(
                            filePath,
                            whileEx.Message,
                            cancellationToken
                        );
                    }
                }

                //  intentionally Disposing and creating scope
                //  so that scope is managed in configurable blocks
                scope.Dispose();
            }
        }
    }
}