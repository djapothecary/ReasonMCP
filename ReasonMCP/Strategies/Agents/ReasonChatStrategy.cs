using DocumentFormat.OpenXml.Office2016.Presentation.Command;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Services;

namespace ReasonMCP.Strategies.Agents
{
    public class ReasonChatStrategy : IChatStrategy
    {
        private readonly ChatHistoryService _chatHistoryService;
        private readonly CurrentChatContextSummarizer _currentContextSummarizer;
        private ChatSettings _settings;
        private readonly ILogger<ReasonChatStrategy> _logger;

        public ReasonChatStrategy
        (
            ChatHistoryService chatHistoryService,
            CurrentChatContextSummarizer currentContextSummarizer,
            IOptions<ChatSettings> options,
            ILogger<ReasonChatStrategy> logger
        )
        {
            _chatHistoryService = chatHistoryService;
            _currentContextSummarizer = currentContextSummarizer;
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

        public async Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatMessageRecord>> LoadChatHistoryAsync()
        {
            var rootDirectory = _settings.RootDirectory;
            var historyDirectory = _settings.ReasonHistoryDirectory;
            var historyFilename = _settings.ReasonHistoryFilename;
            var historyFileExtension = _settings.HistoryFileExtension;

            var fullPath = rootDirectory +
                            historyDirectory +
                            historyFilename +
                            historyFileExtension;

            var chatHistory = await _chatHistoryService.LoadChatHistoryFromFileByAgentAsync(fullPath);

            return await Task.FromResult(chatHistory);
        }

        public async Task<List<ChatMessageRecord>> LoadChatHistoryFromFileAsync()
        {
            var chatHistoryFromFile = new List<ChatMessageRecord>();
            return await Task.FromResult(chatHistoryFromFile);
        }

        public async Task SaveCurrentChatContextAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChatHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ChatHistory> GetSummary(
            ChatHistory currentChatContext
        )
        {
            var summaryThreshold = _settings.ReasonSummarizationThreshold;

            //  TODO:   get system prompt from individual agent YAML file
            var systemPrompt = new ChatMessageContent(AuthorRole.System, "You are a helpful assistant.");
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            return await _currentContextSummarizer.SummarizeCurrentChatContext(
                systemPrompt,
                currentChatContext,
                cancellationToken
            );
        }

        public async Task<List<ChatMessageRecord>> RunAgent()
        {
            var agentResponse = new List<ChatMessageRecord>();
            return agentResponse;
        }
    }
}