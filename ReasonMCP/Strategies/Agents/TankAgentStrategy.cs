using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Configurations;
using ReasonMCP.DTOs;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Services;
using ReasonMPC.Agents;

namespace ReasonMCP.Strategies.Agents
{
    public class TankAgentStrategy : IChatStrategy
    {
        private readonly ChatHistoryService _chatHistoryService;
        private readonly CurrentChatContextSummarizer _currentContextSummarizer;
        private readonly TankAgent _tankAgent;
        private readonly IAgentProfileService _agentProfileService;
        private readonly SessionContextManager _sessionContextManager;
        private readonly ChatSettings _settings;
        private readonly ILogger<TankAgentStrategy> _logger;

        //  File settings
        private readonly string _rootDirectory;
        private readonly string _historyDirectory;
        private readonly string _masterHistoryFilename;
        private readonly string _fileExtension;

        //  variables for tool calling.
        //  when sending to another agent's tool, self should NEVER equal agentId
        private readonly string _self;
        private readonly string _agentId;

        public TankAgentStrategy
        (
            ChatHistoryService chatHistoryService,
            CurrentChatContextSummarizer currentContextSummarizer,
            TankAgent tankAgent,
            IAgentProfileService agentProfileService,
            SessionContextManager sessionContextManager,
            IOptionsMonitor<ChatSettings> options,
            ILogger<TankAgentStrategy> logger
        )
        {
            _chatHistoryService = chatHistoryService;
            _currentContextSummarizer = currentContextSummarizer;
            _tankAgent = tankAgent;
            _agentProfileService = agentProfileService;
            _settings = options.CurrentValue;
            _logger = logger;

            //  Setup files
            _rootDirectory = _settings.RootDirectory;
            _historyDirectory = _settings.ChatHistoryDirectory;
            _masterHistoryFilename = _settings.Agents["tank"].HistoryFilename;
            _fileExtension = _settings.FileExtension;

            //  Agent Identity
            _self = _settings.Agents["tank"].ParticipantId;
            _agentId = _settings.Agents["tank"].ParticipantId;

            //  Initialize session context manager for this strategy instance
            _sessionContextManager = sessionContextManager;
        }

        public string GetMasterHistoryFilePath()
        {
            //  Builds: "masterTankHistory.jsonl"
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
            var fullPath = GetMasterHistoryFilePath();

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
            var agentResponse = new List<ChatMessageRecord>();
            return agentResponse;
        }
    }
}