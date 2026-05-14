using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configuration;
using ReasonMCP.Orchestration;
using ReasonMCP.Tools;

namespace ReasonMCP.Workers
{
    public class DocumentProcessingWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TestingSettings _settings;
        private readonly ILogger<DocumentProcessingWorker> _logger;

        public DocumentProcessingWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<TestingSettings> options,
            ILogger<DocumentProcessingWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Document Ingestion Worker started ...");

            //  This loop runs continously until VS Code is closed or the server is killed
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //  This allows for turning File Processing on/off through appsettings.json
                    if (_settings.EnableEnrichment)
                    {
                        //  1.  Create a fresh scope for this specific run
                        using var scope = _scopeFactory.CreateScope();

                        //  2.  Pre-Process files/locations
                        var preProcessOrchestrator = scope.ServiceProvider.GetRequiredService<PreProcessOrchestrator>();
                        await preProcessOrchestrator.ScanDirectory(cancellationToken);

                        _logger.LogTrace("File scan complete. Sleeping ...");

                        //  3.  Upsert the documents to the vectore store
                        _logger.LogTrace("Starting File Upsert Orchestration ...");

                        // var fileUpsertOrchestrator = scope.ServiceProvider.GetRequiredService<FileUpsertOrchestrator>();
                        // await fileUpsertOrchestrator.ScanMarkdownDirectory(cancellationToken);

                        _logger.LogTrace("File Upser completed.  Sleeping ...");

                        var testKnowledge = scope.ServiceProvider.GetRequiredService<KnowledgeSearchTool>();
                        var resultString = await testKnowledge.SearchKnowledgeBaseASync("Find information about DeepSeaExpoloration.", 5, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    //  Log to stderr so the JSON-RPC stdout stream doesn't get broken
                    _logger.LogError(ex, "[INGESTION_ERROR] Failed during background file scan.");
                }

                //  3.  Sleep for X minutes before checking again.
                //  Using Task.Delay to safely yield the thread back to the CPU
                await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
            }
        }
    }
}