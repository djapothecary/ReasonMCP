using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Configurations;
using ReasonMCP.DTOs;
using ReasonMCP.Interfaces;
using ReasonMCP.Services;

namespace ReasonMCP.Orchestration
{
    public class SemanticKernelWrapperOrchestrator
    {
        private readonly Kernel _kernel;
        private readonly IContextMaintenanceService _contextMaintenance;
        private readonly ChatSettings _settings;
        private readonly ILogger<SemanticKernelWrapperOrchestrator> _logger;
        private readonly CancellationToken cancellationToken;

        public SemanticKernelWrapperOrchestrator(
            Kernel kernel,
            IContextMaintenanceService contextMaintenance,
            IOptions<ChatSettings> options,
            ILogger<SemanticKernelWrapperOrchestrator> logger
        )
        {
            _kernel = kernel;
            _contextMaintenance = contextMaintenance;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> ProcessChatAsync(
            VSCodeChatPayloadDto payload,
            string reasonAgentType
        )
        {
            int summaryThreshold = 0;

            //  1.  Convert DTOs to SK ChatHistory
            var skChathistory = new ChatHistory();

            //  1a. Dynamic Persona routing
            //  currently for testing
            string testResponse = payload.AgentId.ToLower();

            //  2.  Check length -> call _contextMaintenance if needed
            if (payload.History.Count >= summaryThreshold)
            {

                await _contextMaintenance.SummarizeHistory(payload);
            }

            //  3.  Call _kernel.InvokePromptAsync() or IChatCompletionService

            //  4.  Return text

            return testResponse;
        }
    }
}