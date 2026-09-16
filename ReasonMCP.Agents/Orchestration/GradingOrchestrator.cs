using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Agents.Agents;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Records;
using ReasonMCP.Core.Utilities;

namespace ReasonMCP.Agents.Orchestration
{
    public class GradingOrchestrator
    {
        private readonly SeraphAgent _seraphAgent;
        private readonly ChatSettings _settings;
        private readonly ILogger<GradingOrchestrator> _logger;
        private readonly CancellationToken _cancellationToken;

        public GradingOrchestrator
        (
            SeraphAgent seraphAgent,
            IOptionsMonitor<ChatSettings> options,
            ILogger<GradingOrchestrator> logger
        )
        {
            _seraphAgent = seraphAgent;
            _settings = options.CurrentValue;
            _logger = logger;
        }

        public async Task<string> GradeChatHistory(
            ChatHistory chatHistory
        )
        {
            //  Intentionally not using strategy pattern here
            //  grading will always be performed by the Seraph agent
            return string.Empty;
        }
    }
}