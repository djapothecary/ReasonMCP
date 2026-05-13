using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configuration;
using ReasonMCP.Orchestration;
using ReasonMCP.Tools;

namespace ReasonMCP.Workers
{
    public class CodebaseScanWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<CodebaseScanSettings> settings,
        ILogger<CodebaseScanWorker> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Codebase Scan Worker started ...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //  This allows for code base scanning to be turned on/off through appsettings.json
                    if (settings.Value.Enabled)
                    {
                        //  1.  Create a fresh scope for Codebase scanning
                        using var scope = scopeFactory.CreateScope();
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}