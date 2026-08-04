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
            IOptions<CodebaseScanSettings> options,
            IOptions<StorageConfigSettings> storageOptions,
            ILogger<CodebaseWorkflow> logger
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
                var codebaseScanService = scope
                    .ServiceProvider
                    .GetRequiredService<ICodebaseScanService>();

                _logger.LogInformation("Starting Codebase Scan ...");

                await codebaseScanService.ScanCodebaseAsync(
                    cancellationToken
                );

                //  intentionally Disposing and creating scope
                //  so that scope is managed in configurable blocks
                scope.Dispose();
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

                var strategy = scope
                    .ServiceProvider
                    .GetRequiredService<IFileConverterStrategy>();

                int filesConverted = 0;
                int filesToProcess = await ingestionQueue.GetCountIngestedRecordsAsync(
                    "Codebase",
                    cancellationToken
                );

                while (filesConverted < filesToProcess)
                {
                    var fileIngestionRecord = await codebaseProcessor
                                .GetNextCodebaseFileAsync(
                                cancellationToken
                            );

                    var filePath = fileIngestionRecord.FilePath;

                    try
                    {
                        //  1.  Get the next file to process by "TargetStore = "Codebase" "
                        var file = await ingestionQueue.DequeueNextFileAsync(
                            "Codebase",
                            cancellationToken
                        );

                        //  preventing end of run exception
                        if (file == null)
                            return;

                        bool convertSuccess;

                        convertSuccess = await strategy
                            .ConvertForIngestionAsync(
                                file.FilePath,
                                cancellationToken
                            );

                        if (convertSuccess)
                        {
                            await ingestionQueue.MarkConversionCompleteAsync(
                                file.FilePath,
                                cancellationToken
                            );
                        }
                        else
                        {
                            await ingestionQueue.MarkConversionFailedAsync(
                                file.FilePath,
                                "Codebase upsert failed",
                                cancellationToken
                            );
                        }
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