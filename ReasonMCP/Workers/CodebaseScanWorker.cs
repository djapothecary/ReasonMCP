using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configuration;
using ReasonMCP.Orchestration;
using ReasonMCP.Tools;

namespace ReasonMCP.Workers
{
    public class CodebaseScanWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<CodebaseScanWorker> _logger;

        public CodebaseScanWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<CodebaseScanSettings> options,
            ILogger<CodebaseScanWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Codebase Scan Worker started ...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //  This allows for code base scanning to be turned on/off through appsettings.json
                    if (_settings.Enabled)
                    {
                        //  1.  Create a fresh scope for Codebase scanning
                        using var scope = _scopeFactory.CreateScope();

                        //  2.  Scan Codebase directories
                        var codebaseOrchestrator = scope.ServiceProvider.GetRequiredService<CodebaseScanOrchestrator>();
                        await codebaseOrchestrator.ScanCodebaseAsync(cancellationToken);

                        _logger.LogTrace("Codebase scan complete. Sleeping ...");
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}