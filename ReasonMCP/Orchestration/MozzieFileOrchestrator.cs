using Microsoft.Extensions.DependencyInjection;
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
    public class MozzieFileOrchestrator
    {
        private readonly IEnumerable<IChatStrategy> _chatStrategies;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IChatHistoryService _chatHistoryService;
        private readonly StorageConfigSettings _storageSettings;
        private readonly ILogger<MozzieFileOrchestrator> _logger;

        public MozzieFileOrchestrator(
            IEnumerable<IChatStrategy> chatStrategies,
            IServiceScopeFactory scopeFactory,
            IChatHistoryService chatHistoryService,
            IOptions<StorageConfigSettings> storageSettings,
            ILogger<MozzieFileOrchestrator> logger
        )
        {
            _chatStrategies = chatStrategies;
            _scopeFactory = scopeFactory;
            _chatHistoryService = chatHistoryService;
            _storageSettings = storageSettings.Value;
            _logger = logger;
        }



    }
}