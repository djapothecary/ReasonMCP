using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configuration;
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
        private readonly IEnumerable<IFileConverterStrategy> _strategies;
        private readonly IFileConverterUtility _fileConverter;
        private readonly KnowledgebaseScanSettings _settings;
        private readonly ILogger<KnowledgebaseScanWorker> _logger;

        public KnowledgebaseScanWorker(
            IServiceScopeFactory scopeFactory,
            IIngestionQueueService ingestionQueue,
            IEnumerable<IFileConverterStrategy> strategies,
            IFileConverterUtility fileConverter,
            IOptions<KnowledgebaseScanSettings> options,
            ILogger<KnowledgebaseScanWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _ingestionQueue = ingestionQueue;
            _strategies = strategies;
            _fileConverter = fileConverter;
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
                var file = new FileIngestionRecord();
                try
                {
                    var ingestionQueue = scope.ServiceProvider.GetRequiredService<DapperIngestionQueueService>();

                    //  3.  Get the next Knowledgebase document by TargetStore = Doscuments
                    file = await ingestionQueue.DequeueNextFileAsync("Documents", cancellationToken);

                    bool convertSuccesss;

                    //  4.  Determine file type and what processor to use
                    var strategy = _strategies.FirstOrDefault(s => s.CanConvert(file!.FilePath));

                    //  5.  Convert file to markdown
                    convertSuccesss = await strategy!.ConvertForIngestionAsync(file!.FilePath);

                    if (convertSuccesss)
                    {
                        await _fileConverter.ClearOriginalFile(file!.FilePath);
                        await _ingestionQueue.MarkCompleteAsync(file!.FilePath, cancellationToken);
                    }
                    else
                    {
                        await _ingestionQueue.MarkFailedAsync(file!.FilePath!, "Upsert failed", cancellationToken);
                    }

                    //  6.  Get the file for Upsert
                    //  This will handle the embeddings and Upsert to the vector store
                    var fileUpsertOrchestrator = scope.ServiceProvider.GetRequiredService<KnowledgebaseRecordUpsertOrchestrator>();
                    await fileUpsertOrchestrator.GetFileForUpsertFromIngestQueueAsync(file!.FilePath!, cancellationToken);

                    //  7.  Update the IngestionQueue to indicate that the process is complete
                    await ingestionQueue.MarkCompleteAsync(file!.FilePath, cancellationToken);

                }
                catch (Exception whileEx)
                {
                    _logger.LogError("An error occurred while performing Knowledgebase document ingestion: {whileEx}", whileEx);
                    await _ingestionQueue.MarkFailedAsync(file!.FilePath, whileEx.Message, cancellationToken);
                }
            }
        }
    }
}
