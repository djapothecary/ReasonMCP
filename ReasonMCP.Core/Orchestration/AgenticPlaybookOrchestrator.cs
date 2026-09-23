using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;

namespace ReasonMCP.Core.Orchestration
{
    public class AgenticPlaybookOrchestrator
    {
        private readonly IPlaybookWorkflow _playbookWorkflow;
        private readonly StorageConfigSettings _storageSettings;
        private readonly ILogger<AgenticPlaybookOrchestrator> _logger;

        public AgenticPlaybookOrchestrator(
            IPlaybookWorkflow playbookWorkflow,
            IOptionsMonitor<StorageConfigSettings> storageSettings,
            ILogger<AgenticPlaybookOrchestrator> logger
        )
        {
            _playbookWorkflow = playbookWorkflow;
            _storageSettings = storageSettings.CurrentValue;
            _logger = logger;
        }

        public async Task ProcessPlaybookAsync(
            string playbookFilepath
        )
        {
            if (playbookFilepath == string.Empty)
                return;

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            //  Create a sessionId
            var sessionId = Guid.NewGuid().ToString();
            var userCommand = "Started";

            var playbookResponse = await _playbookWorkflow.ProcessPlaybookWorkflowAsync(
                playbookFilepath,
                sessionId,
                userCommand,
                cancellationToken
            );
        }

    }
}