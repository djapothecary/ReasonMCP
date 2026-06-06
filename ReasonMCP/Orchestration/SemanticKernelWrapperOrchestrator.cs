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
        private readonly IEnumerable<IChatStrategy> _strategies;
        private readonly IContextMaintenanceService _contextMaintenance;
        private readonly ChatSettings _settings;
        private readonly ILogger<SemanticKernelWrapperOrchestrator> _logger;
        private readonly CancellationToken cancellationToken;

        public SemanticKernelWrapperOrchestrator(
            Kernel kernel,
            IEnumerable<IChatStrategy> strategies,
            IContextMaintenanceService contextMaintenance,
            IOptions<ChatSettings> options,
            ILogger<SemanticKernelWrapperOrchestrator> logger
        )
        {
            _kernel = kernel;
            _strategies = strategies;
            _contextMaintenance = contextMaintenance;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> ProcessChatAsync(
            VSCodeChatPayloadDto payload,
            string reasonAgentType
        )
        {
            //  1.  Convert DTOs to SK ChatHistory
            var skChathistory = new ChatHistory();
            var currentChatSummary = new ChatHistory();

            foreach (var turn in payload.History)
            {
                skChathistory.AddMessage(new AuthorRole(turn.Role), turn.Content);
            }

            //  2. Dynamic Persona routing
            string agentId = payload.AgentId.ToLower();
            var agentStrategy = _strategies.FirstOrDefault(s => s.GetAgentStrategy(agentId));

            //  3.  Add current message to "master" chat history regardless


            //  4. Determine if summary needed
            var turnCount = payload.History.Count(m => m.Role == "user");
            var shouldSummarize = agentStrategy!.ShouldSummarize(turnCount);

            if (shouldSummarize)
            {
                //  perform current chat context summarization
                currentChatSummary = await agentStrategy!.GetSummary(skChathistory);
            }

            //  3.  Call _kernel.InvokePromptAsync() or IChatCompletionService

            //  4.  Return text

            return String.Empty;
        }
    }
}