using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Workflows;

namespace ReasonMCP.Workers
{
    public class EnrichmentWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TestingSettings _settings;
        private readonly ILogger<EnrichmentWorker> _logger;

        public EnrichmentWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<TestingSettings> options,
            ILogger<EnrichmentWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        //  Entry point for Reference Scan
        //  Calls the Reference workflow
        protected override async Task ExecuteAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (!_settings.EnableEnrichment)
                return;

            _logger.LogInformation("Enrichment Worker started ...");

            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                // Resolve your newly renamed workflows
                var codebaseWf = scope
                    .ServiceProvider
                    .GetRequiredService<CodebaseWorkflow>();

                var documentWf = scope
                    .ServiceProvider
                    .GetRequiredService<DocumentWorkflow>();

                var referenceWf = scope
                    .ServiceProvider
                    .GetRequiredService<ReferenceWorkflow>();

                try
                {
                    // THE MULTI-THREADING BOOST: Run all three ingestion pipelines concurrently!
                    var codebaseTask = codebaseWf.RunAsync(cancellationToken);
                    var documentTask = documentWf.RunAsync(cancellationToken);
                    var referenceTask = referenceWf.RunAsync(cancellationToken);

                    await Task.WhenAll(codebaseTask, documentTask, referenceTask);
                }
                catch (Exception ex)
                {
                    // If one crashes, it doesn't take down the hosted service!
                    _logger.LogError(ex, "An error occurred during an enrichment workflow.");
                }

                // Sleep until the next scheduled scan (e.g., every 4 hours)
                await Task.Delay(TimeSpan.FromHours(4), cancellationToken);
            }
        }
    }
}
