using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Agents;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Services;

namespace ReasonMCP.Strategies.Agents
{
    public class ReasonAgentStrategy : IChatStrategy
    {
        private readonly ChatHistoryService _chatHistoryService;
        private readonly CurrentChatContextSummarizer _currentContextSummarizer;
        private readonly ReasonAgent _reasonAgent;
        private readonly IAgentProfileService _agentProfileService;
        private ChatSettings _settings;
        private readonly ILogger<ReasonAgentStrategy> _logger;

        //  File settings
        private readonly string _rootDirectory;
        private readonly string _historyDirectory;
        private readonly string _contextFileName;
        private readonly string _masterHistoryFilename;
        private readonly string _fileExtension;

        //  variables for tool calling.
        //  when sending to another agent's tool, self should NEVER equal agentId
        private readonly string _self;
        private readonly string _agentId;

        public ReasonAgentStrategy
        (
            ChatHistoryService chatHistoryService,
            CurrentChatContextSummarizer currentContextSummarizer,
            ReasonAgent reasonAgent,
            IAgentProfileService agentProfileService,
            IOptions<ChatSettings> options,
            ILogger<ReasonAgentStrategy> logger
        )
        {
            _chatHistoryService = chatHistoryService;
            _currentContextSummarizer = currentContextSummarizer;
            _reasonAgent = reasonAgent;
            _agentProfileService = agentProfileService;
            _settings = options.Value;
            _logger = logger;

            //  Setup files
            _rootDirectory = _settings.RootDirectory;
            _historyDirectory = _settings.ReasonHistoryDirectory;
            _contextFileName = _settings.ReasonCurrentContextFilename;
            _masterHistoryFilename = _settings.ReasonHistoryFilename;
            _fileExtension = _settings.HistoryFileExtension;

            //  Agent Identity
            _self = _settings.ReasonParticipantId;
            _agentId = _settings.ReasonParticipantId;
        }

        public string GenerateCurrentContextFilePath()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8); // 8 char hex

            //  Builds: "reasonContext_2026_06_06_143800_a1b2c3d4.jsonl"
            var currentContextFilename = $"{_contextFileName}_{timestamp}_{shortGuid}{_fileExtension}";

            //  Can't use Path.Combine() because literal paths always return the last path
            // return Path.Combine(_rootDirectory, _historyDirectory, currentContextFilename);
            return _rootDirectory + _historyDirectory + currentContextFilename;
        }

        public string GetMasterHistoryFilePath()
        {
            //  Builds: "masterReasonHistory.jsonl"
            var masterHistoryFilename = $"{_masterHistoryFilename}{_fileExtension}";

            //  Can't use Path.Combine() because literal paths always return the last path
            // return Path.Combine(_rootDirectory, _historyDirectory, masterHistoryFilename);
            return _rootDirectory + _historyDirectory + masterHistoryFilename;
        }

        public bool GetAgentStrategy(string agent)
        {
            return agent.Equals(_settings.ReasonParticipantId, StringComparison.OrdinalIgnoreCase);
        }

        public bool ShouldSummarize(int turnCount)
        {
            return turnCount > _settings.ReasonSummarizationThreshold;
        }

        public async Task AppendToChathistory(
            ChatMessageRecord record
        )
        {
            var fullPath = GetMasterHistoryFilePath();

            await _chatHistoryService
                    .AppendToHistoryFileAsync(
                        record,
                        fullPath);
        }

        public async Task AppendToCurrentContext(
            ChatMessageRecord record
        )
        {
            var fullPath = GenerateCurrentContextFilePath();

            await _chatHistoryService
                    .AppendToHistoryFileAsync(
                        record,
                        fullPath);
        }

        public async Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatMessageRecord>> LoadChatHistoryAsync()
        {

            var fullPath = GetMasterHistoryFilePath();

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
            var systemPrompt = new ChatMessageContent(
                                AuthorRole.System, "You are a helpful assistant.");
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            return await _currentContextSummarizer.SummarizeCurrentChatContext(
                systemPrompt,
                currentChatContext,
                cancellationToken
            );
        }

        public async Task<List<ChatMessageRecord>> RunAgent(
            ChatHistory currentContext,
            string prompt
        )
        {
            var agentProfile = await _agentProfileService.LoadAgentProfileAsync(_settings.ReasonAgentProfilePath);
            var agentResponse = await _reasonAgent.SendPrompt(
                                agentProfile,
                                currentContext,
                                prompt);

            return agentResponse;
        }
    }
}