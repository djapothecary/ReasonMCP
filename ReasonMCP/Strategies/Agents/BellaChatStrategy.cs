using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Services;

namespace ReasonMCP.Strategies.Agents
{
    public class BellaChatStrategy : IChatStrategy
    {
        private readonly IChatHistoryService _chatHistoryService;
        private ChatSettings _settings;
        private readonly ILogger<BellaChatStrategy> _logger;

        public BellaChatStrategy
        (
            IChatHistoryService chatHistoryService,
            IOptions<ChatSettings> options,
            ILogger<BellaChatStrategy> logger
        )
        {
            _chatHistoryService = chatHistoryService;
            _settings = options.Value;
            _logger = logger;
        }

        public bool GetAgentStrategy(string agent)
        {
            return agent.Equals(_settings.BellaParticipantId, StringComparison.OrdinalIgnoreCase);
        }

        public bool ShouldSummarize(int turnCount)
        {
            return turnCount > _settings.BellaSummarizationThreshold;
        }

        public async Task<List<ChatMessageRecord>> LoadChatHistoryAsync()
        {
            var rootDirectory = _settings.RootDirectory;
            var bellaChatHistoryDirectory = _settings.BellaHistoryDirectory;
            var bellaHistoryFilename = _settings.BellaHistoryFilename;
            var fileExtension = _settings.HistoryFileExtension;

            var fullPath = rootDirectory +
                            bellaChatHistoryDirectory +
                            bellaHistoryFilename +
                            fileExtension;

            var bellaChatHistory = await _chatHistoryService.LoadChatHistoryByAgentAsync(fullPath);

            return bellaChatHistory;
        }

        public async Task SaveChatHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveCurrentChatContextAsync()
        {
            throw new NotImplementedException();
        }
    }
}