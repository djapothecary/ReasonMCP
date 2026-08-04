using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;

namespace ReasonMCP.Workflows
{
    public class ReferenceWorkflow
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ReferenceScanSettings _settings;
        private readonly StorageConfigSettings _storageSettings;
        private readonly ILogger<ReferenceWorkflow> _logger;

        public ReferenceWorkflow(
            IServiceScopeFactory scopeFactory,
            IOptions<ReferenceScanSettings> options,
            IOptions<StorageConfigSettings> storageOptions,
            ILogger<ReferenceWorkflow> logger
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

            //  1.  Call service to scan directories
            //  this will also perform upsert to ingestion queue
            if (_settings.RunFileScan)
            {
                var scope = _scopeFactory.CreateScope();
                var referenceScanService = scope
                    .ServiceProvider
                    .GetRequiredService<IReferenceDataScanService>();

                _logger.LogInformation("Starting Reference Data Scan ...");

                await referenceScanService.ScanReferenceDataAsync(
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

                var refDataProcessor = scope
                    .ServiceProvider
                    .GetRequiredService<IReferenceDataProcessor>();

                var strategies = scope
                    .ServiceProvider
                    .GetRequiredService<IEnumerable<IFileConverterStrategy>>();

                var fileConverter = scope
                    .ServiceProvider
                    .GetRequiredService<IFileConverterUtility>();

                int filesConverted = 0;
                int filesToProcess = await ingestionQueue.GetCountIngestedRecordsAsync(
                    "Reference",
                    cancellationToken
                );

                while (filesConverted < filesToProcess)
                {
                    var fileIngestionRecord = await refDataProcessor
                                .GetNextReferenceFileAsync(
                                cancellationToken
                            );

                    var filePath = fileIngestionRecord.FilePath;

                    try
                    {
                        //  get the strategies to enrich the Reference file

                        //  Determine file type and what processor to use
                        var strategy = strategies.FirstOrDefault(
                            s => s.CanConvert(
                                fileIngestionRecord.FilePath
                            )
                        );

                        bool convertSuccess;

                        //  perform enrichment
                        convertSuccess = await strategy!
                            .ConvertForIngestionAsync(
                                filePath,
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
                                "Reference file conversion failed",
                                cancellationToken
                            );
                        }

                        //  Clear original files (or not)
                        if (_storageSettings.ClearOriginalFile)
                        {
                            await fileConverter.ClearOriginalFile(filePath);
                        }

                        filesConverted++;
                    }
                    catch (Exception whileEx)
                    {
                        _logger.LogError(
                            $"An error occurred while performing Reference document ingestion: {whileEx.Message}",
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

            //  4.  embed
            if (_settings.GenerateEmbeddings)
            {
                var scope = _scopeFactory.CreateScope();

                var ingestionQueue = scope
                    .ServiceProvider
                    .GetRequiredService<IIngestionQueueService>();

                var refDataProcessor = scope
                    .ServiceProvider
                    .GetRequiredService<IReferenceDataProcessor>();

                int filesProcessed = 0;
                int filesToProcess = await ingestionQueue.GetCountConvertedRecordsAsync(
                    "Reference",
                    cancellationToken
                );

                bool embedSuccess = false;

                while (filesProcessed < filesToProcess)
                {
                    var fileToEmbed = await ingestionQueue.DequeueNextFileToEmbedAsync(
                        "Reference",
                        cancellationToken
                    );

                    var filePath = fileToEmbed!.FilePath;

                    try
                    {
                        embedSuccess = await refDataProcessor.IngestReferenceFileRecordAsync(
                        filePath,
                        cancellationToken
                    );

                        //  5.  mark complete
                        if (embedSuccess)
                        {
                            await ingestionQueue.MarkCompleteAsync(
                                filePath,
                                cancellationToken
                            );
                        }
                        else
                        {
                            await ingestionQueue.MarkIngestionFailedAsync(
                                filePath,
                                "Reference FileRecord embedding failed",
                                cancellationToken
                            );
                        }

                        filesProcessed++;
                    }
                    catch (Exception whileEx)
                    {
                        _logger.LogError(
                            $"An error occurred while performing Reference document ingestion: {whileEx.Message}",
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