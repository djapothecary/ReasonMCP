using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReasonMCP.Orchestration;

namespace ReasonMCP.Workers
{
    public class DocumentProcessingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentProcessingWorker> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Document Ingestion Worker started.");

            //  This loop runs continously until VS Code is closed or the server is killed
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //  1.  Create a fresh scope for this specific run
                    using var scope = scopeFactory.CreateScope();

                    //  2.  Pre-Process files/locations
                    var preProcessOrchestrator = scope.ServiceProvider.GetRequiredService<PreProcessOrchestrator>();
                    await preProcessOrchestrator.ScanDirectory(cancellationToken);

                    logger.LogTrace("File scan complete. Sleeping ...");

                    //  3.  Upsert the documents to the vectore store
                    logger.LogTrace("Starting File Upsert Orchestration ...");

                    var fileUpsertOrchestrator = scope.ServiceProvider.GetRequiredService<FileUpsertOrchestrator>();
                    await fileUpsertOrchestrator.ScanMarkdownDirectory(cancellationToken);

                    logger.LogTrace("File Upser completed.  Sleeping ...");
                }
                catch (Exception ex)
                {
                    //  Log to stderr so the JSON-RPC stdout stream doesn't get broken
                    logger.LogError(ex, "[INGESTION_ERROR] Failed during background file scan.");
                }

                //  3.  Sleep for X minutes before checking again.
                //  Using Task.Delay to safely yield the thread back to the CPU
                await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
            }
        }
    }
}