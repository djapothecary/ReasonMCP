using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Agents;
using ReasonMCP.Configurations;
using ReasonMCP.DTOs;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Utilities;

namespace ReasonMCP.Orchestration
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
            IOptions<ChatSettings> options,
            ILogger<GradingOrchestrator> logger
        )
        {
            _seraphAgent = seraphAgent;
            _settings = options.Value;
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