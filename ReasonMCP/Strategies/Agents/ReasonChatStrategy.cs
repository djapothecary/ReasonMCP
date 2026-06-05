using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Services;

namespace ReasonMCP.Strategies.Agents
{
    public class ReasonChatStrategy : IChatStrategy
    {
        private readonly ChatHistoryService _chatHistoryService;
        private ChatSettings _settings;
        private readonly ILogger<ReasonChatStrategy> _logger;

        public ReasonChatStrategy
        (
            ChatHistoryService chatHistoryService,
            IOptions<ChatSettings> options,
            ILogger<ReasonChatStrategy> logger
        )
        {
            _chatHistoryService = chatHistoryService;
            _settings = options.Value;
            _logger = logger;
        }

        public bool GetAgentStrategy(string agent)
        {
            return agent.Equals(_settings.ReasonParticipantId, StringComparison.OrdinalIgnoreCase);
        }

        public bool ShouldSummarize(int turnCount)
        {
            return turnCount > _settings.ReasonSummarizationThreshold;
        }

        public Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<ChatMessageRecord>> LoadChatHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveCurrentChatContextAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveChatHistoryAsync()
        {
            throw new NotImplementedException();
        }
    }
}