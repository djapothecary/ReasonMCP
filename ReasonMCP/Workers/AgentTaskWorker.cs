using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Orchestration;

namespace ReasonMCP.Workers
{
    public class AgentTaskWorker : BackgroundService
    {
        private readonly Channel<dynamic> _agentTaskChannel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AgentTaskWorkerSettings _settings;
        private readonly ILogger<AgentTaskWorker> _logger;

        public AgentTaskWorker
        (
            Channel<dynamic> agentTaskChannel,
            IServiceScopeFactory scopeFactory,
            IOptions<AgentTaskWorkerSettings> options,
            ILogger<AgentTaskWorker> logger
        )
        {
            _agentTaskChannel = agentTaskChannel;
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

            _logger.LogInformation("Agent Task worker has started ...");

            //  Run the Agent Task worker continously until VS Code is closed or the server is killed
            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (var task in _agentTaskChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        //  route to the Agent Orchestrator
                        if (task.TaskType == "GradeResponse")
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background agent failed.");
                    }
                }
            }
        }
    }
}