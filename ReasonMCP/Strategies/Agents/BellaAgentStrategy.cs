using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Agents;
using ReasonMCP.Configurations;
using ReasonMCP.DTOs;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Services;

namespace ReasonMCP.Strategies.Agents
{
    public class BellaAgentStrategy : IChatStrategy
    {
        private readonly ChatHistoryService _chatHistoryService;
        private readonly CurrentChatContextSummarizer _currentContextSummarizer;
        private readonly BellaAgent _bellaAgent;
        private readonly IAgentProfileService _agentProfileService;
        private readonly SessionContextManager _sessionContextManager;
        private readonly ChatSettings _settings;
        private readonly ILogger<BellaAgentStrategy> _logger;

        //  File settings
        private readonly string _rootDirectory;
        private readonly string _historyDirectory;
        private readonly string _masterHistoryFilename;
        private readonly string _fileExtension;

        //  variables for tool calling.
        //  when sending to another agent's tool, self should NEVER equal agentId
        private readonly string _self;
        private readonly string _agentId;

        public BellaAgentStrategy
        (
            ChatHistoryService chatHistoryService,
            CurrentChatContextSummarizer currentContextSummarizer,
            BellaAgent bellaAgent,
            IAgentProfileService agentProfileService,
            SessionContextManager sessionContextManager,
            IOptionsMonitor<ChatSettings> options,
            ILogger<BellaAgentStrategy> logger
        )
        {
            _chatHistoryService = chatHistoryService;
            _currentContextSummarizer = currentContextSummarizer;
            _bellaAgent = bellaAgent;
            _agentProfileService = agentProfileService;
            _settings = options.CurrentValue;
            _logger = logger;

            //  Setup files
            _rootDirectory = _settings.RootDirectory;
            _historyDirectory = _settings.ChatHistoryDirectory;
            _masterHistoryFilename = _settings.Agents["bella"].HistoryFilename;
            _fileExtension = _settings.FileExtension;

            //  Agent Identity
            _self = _settings.Agents["bella"].ParticipantId;
            _agentId = _settings.Agents["bella"].ParticipantId;

            //  Initialize session context manager for this strategy instance
            _sessionContextManager = sessionContextManager;
        }

        public string GetMasterHistoryFilePath()
        {
            //  Builds: "masterBellaHistory.jsonl"
            var masterHistoryFilename = $"{_masterHistoryFilename}{_fileExtension}";

            //  Can't use Path.Combine() because literal paths always return the last path
            // return Path.Combine(_rootDirectory, _historyDirectory, masterHistoryFilename);
            return _rootDirectory + _historyDirectory + masterHistoryFilename;
        }

        public bool GetAgentStrategy(string agent)
        {
            return agent.Equals(_self, StringComparison.OrdinalIgnoreCase);
        }

        public bool ShouldSummarize(int turnCount)
        {
            return turnCount > _settings.ChatSummarizationThreshold;
        }

        public async Task AppendToChathistory(
            ChatMessageRecord record
        )
        {
            var fullPath = GetMasterHistoryFilePath();  // Uses session-persisted file

            await _chatHistoryService
                    .AppendToHistoryFileAsync(
                        record,
                        fullPath);
        }

        public async Task AppendToCurrentContext(
            ChatMessageRecord record,
            VSCodeChatPayloadDto payload
        )
        {
            var fullPath = _sessionContextManager.GetCurrentContextFilePath(
                payload.AgentId,
                payload.SessionId
            );

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

        public async Task<List<ChatMessageRecord>> RunAgent(
            VSCodeChatPayloadDto payload,
            ChatHistory currentContext,
            string prompt
        )
        {
            var currentContextFilepath = _sessionContextManager.GetCurrentContextFilePath(
                payload.AgentId,
                payload.SessionId
            );

            var agentProfile = await _agentProfileService.LoadAgentProfileAsync(_settings.Agents["bella"].AgentProfilePath);
            var agentResponse = await _bellaAgent.SendPrompt(
                                agentProfile,
                                currentContext,
                                prompt,
                                currentContextFilepath);

            return agentResponse;
        }
    }
}