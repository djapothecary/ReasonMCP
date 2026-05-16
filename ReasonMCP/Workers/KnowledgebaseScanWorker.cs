using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configuration;
using ReasonMCP.Orchestration;
using ReasonMCP.Services;

namespace ReasonMCP.Workers
{
    public class KnowledgebaseScanWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KnowledgebaseScanSettings _settings;
        private readonly ILogger<KnowledgebaseScanWorker> _logger;

        public KnowledgebaseScanWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<KnowledgebaseScanSettings> options,
            ILogger<KnowledgebaseScanWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
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
                try
                {
                    var ingestionQueue = scope.ServiceProvider.GetRequiredService<DapperIngestionQueueService>();

                    //  3.  Get the next Knowledgebase document by TargetStore = Doscuments
                    //var file = await ingestionQueue.DequeueNextFileAsync("Documents", cancellationToken);
                    var file = @"C:\Source\ReasonData\ADRS\DeepSeaExploration.txt";

                    //  4.  Convert Knowledgebase record to Markdown and clear original file
                    var preprocessOrchestrator = scope.ServiceProvider.GetRequiredService<PreProcessOrchestrator>();
                    // await preprocessOrchestrator.PreprocessFileAsync(file!.FilePath, cancellationToken);
                    await preprocessOrchestrator.PreprocessFileFromQueueAsync(file, cancellationToken);

                    //  5.  Get the file for Upsert
                    //  This will handle the embeddings and Upsert to the vector store
                    var fileUpsertOrchestrator = scope.ServiceProvider.GetRequiredService<KnowledgebaseRecordUpsertOrchestrator>();
                    // await fileUpsertOrchestrator.GetFileForUpsert(file!.FilePath, cancellationToken);
                    await fileUpsertOrchestrator.GetFileForUpsertFromIngestQueueAsync(file, cancellationToken);

                    //  6.  Update the IngestionQueue to indicate that the process is complete
                    await ingestionQueue.MarkCompleteAsync(file, cancellationToken);

                }
                catch (Exception whileEx)
                {
                    _logger.LogError("An error occurred while performing Knowledgebase document ingestion: {whileEx}", whileEx);
                }
            }
        }
    }
}
