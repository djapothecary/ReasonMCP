using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;

namespace ReasonMCP.Workflows
{
    public class CodebaseWorkflow
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CodebaseScanSettings _settings;
        private readonly StorageConfigSettings _storageSettings;
        private readonly ILogger<CodebaseWorkflow> _logger;

        public CodebaseWorkflow(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<CodebaseScanSettings> options,
            IOptionsMonitor<StorageConfigSettings> storageOptions,
            ILogger<CodebaseWorkflow> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.CurrentValue;
            _storageSettings = storageOptions.CurrentValue;
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
                using var scope = _scopeFactory.CreateScope();
                var codebaseScanService = scope
                    .ServiceProvider
                    .GetRequiredService<ICodebaseScanService>();

                _logger.LogInformation("Starting Codebase Scan ...");

                await codebaseScanService.ScanCodebaseAsync(
                    cancellationToken
                );
            }

            //  2.  Dequeue nextt record
            if (_settings.ProcessFiles)
            {
                var scope = _scopeFactory.CreateScope();

                var ingestionQueue = scope
                    .ServiceProvider
                    .GetRequiredService<IIngestionQueueService>();

                var codebaseProcessor = scope
                    .ServiceProvider
                    .GetRequiredService<ICodebaseProcessor>();

                var strategies = scope
                    .ServiceProvider
                    .GetRequiredService<IEnumerable<IFileConverterStrategy>>();

                var fileConverter = scope
                    .ServiceProvider
                    .GetRequiredService<IFileConverterUtility>();

                int filesprocessed = 0;
                int filesToProcess = await ingestionQueue.GetCountIngestedRecordsAsync(
                    "Codebase",
                    cancellationToken
                );

                string filePath = string.Empty;

                while (filesprocessed < filesToProcess)
                {

                    try
                    {
                        //  1.  Get the next file to process by "TargetStore = "Codebase" "
                        var fileIngestionRecord = await codebaseProcessor
                                .GetNextCodebaseFileAsync(
                                cancellationToken
                            );

                        filePath = fileIngestionRecord.FilePath;

                        //  Determine file type and what processor to use
                        var strategy = strategies.FirstOrDefault(
                            s => s.CanConvert(
                                filePath
                            )
                        );

                        //  preventing end of run exception
                        if (filePath == null)
                            return;

                        bool convertSuccess;
                        bool writeConvertedOutput = _settings.WriteConvertedOutput;

                        convertSuccess = await strategy!
                            .ConvertForIngestionAsync(
                                filePath,
                                writeConvertedOutput,
                                cancellationToken
                            );

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
                                "Codebase upsert failed",
                                cancellationToken
                            );
                        }

                        Console.WriteLine(filePath);
                        filesprocessed++;
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
            }
        }
    }
}