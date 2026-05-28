using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Orchestration;
using ReasonMCP.Services;

namespace ReasonMCP.Workers
{
    public class KnowledgebaseScanWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestionQueueUpdaterService _updaterService;
        private readonly IEnumerable<IFileConverterStrategy> _strategies;
        private readonly IFileConverterUtility _fileConverter;
        private readonly KnowledgebaseScanSettings _settings;
        private readonly StorageConfig _storageSettings;
        private readonly ILogger<KnowledgebaseScanWorker> _logger;

        public KnowledgebaseScanWorker(
            IServiceScopeFactory scopeFactory,
            IIngestionQueueService ingestionQueue,
            IIngestionQueueUpdaterService updaterService,
            IEnumerable<IFileConverterStrategy> strategies,
            IFileConverterUtility fileConverter,
            IOptions<KnowledgebaseScanSettings> options,
            IOptions<StorageConfig> storageOptions,
            ILogger<KnowledgebaseScanWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _ingestionQueue = ingestionQueue;
            _updaterService = updaterService;
            _strategies = strategies;
            _fileConverter = fileConverter;
            _settings = options.Value;
            _storageSettings = storageOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (!_settings.Enabled)
                return;

            _logger.LogInformation("Knowledgebbase Scan Worker started ...");

            //  1.  Create a fresh scope for Knowledgebase scanning
            using var scope = _scopeFactory.CreateScope();

            try
            {
                //  2.  Scan KnowledgeBase directories
                var knowledgebaseOrchestrator = scope.ServiceProvider.GetRequiredService<KnowledgebaseScanOrchestrator>();
                await knowledgebaseOrchestrator.ScanKnowledgebaseAsync(cancellationToken);

                _logger.LogTrace("Knowledgebase scan complete. Sleeping ...");
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while performing Knowledgebase scan: {ex}", ex);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var file = new FileIngestionRecord();
                try
                {
                    //  3.  Get the next Knowledgebase document by TargetStore = Doscuments
                    file = await _ingestionQueue.DequeueNextFileAsync("Documents", cancellationToken);

                    bool convertSuccesss;

                    //  bail out now if file is null
                    if (file == null)
                        return;

                    //  4.  Determine file type and what processor to use
                    var strategy = _strategies.FirstOrDefault(s => s.CanConvert(file!.FilePath));

                    //  5.  Convert file to markdown
                    convertSuccesss = await strategy!.ConvertForIngestionAsync(file!.FilePath);

                    //  Mark conversion status
                    await _updaterService.MarkConversionStatus(file.FilePath, convertSuccesss, cancellationToken);

                    //  Clear original files (or not)
                    if (_storageSettings.ClearOriginalFile)
                    {
                        await _fileConverter.ClearOriginalFile(file!.FilePath);
                    }

                    //  6.  Get the file for Upsert
                    //  This will handle the embeddings and Upsert to the vector store
                    var fileUpsertOrchestrator = scope.ServiceProvider.GetRequiredService<KnowledgebaseRecordUpsertOrchestrator>();
                    await fileUpsertOrchestrator.GetFileForUpsertFromIngestQueueAsync(file!.FilePath!, cancellationToken);

                    //  7.  Update the IngestionQueue to indicate that the process is complete
                    await _ingestionQueue.MarkCompleteAsync(file!.FilePath, cancellationToken);

                }
                catch (Exception whileEx)
                {
                    _logger.LogError("An error occurred while performing Knowledgebase document ingestion: {whileEx}", whileEx);
                    await _ingestionQueue.MarkFailedExceptionAsync(file!.FilePath, whileEx.Message, cancellationToken);
                }
            }
        }
    }
}
