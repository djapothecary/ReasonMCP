using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Agents.Tools;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Records;
using ReasonMCP.Core.Services;
using ReasonMCP.Core.Utilities;

namespace ReasonMCP.Agents.Agents
{
    /// <summary>
    /// GLaDOS’s entire purpose in the Portal universe is to construct
    /// highly complex, rigidly structured "Test Chambers" for the test subjects
    /// to navigate.
    /// In the ReasonMCP architecture, a Jira ticket or a vague human requirement is the
    /// input.GLaDOS parses it and constructs a rigid, multi-step Markdown/XML
    /// "Test Chamber" (the Playbook). Then, she hands that playbook over to
    /// Tank (the Operator) and Reason (the Test Subject) to actually execute
    /// and survive.It is a brilliant metaphor.
    public class GladosAgent : IGladosAgent
    {
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IAgentProfileService _agentProfileService;
        private readonly SessionContextManager _sessionContextManager;
        private readonly ChatSettings _settings;
        private readonly PlaybookSettings _playbookSettings;
        private readonly ILogger<GladosAgent> _logger;

        public GladosAgent(

            Kernel kernel,
            IServiceProvider serviceProvider,
            [FromKeyedServices("MnemosyneService")] IChatCompletionService chatCompletionService,
            IAgentProfileService agentProfileService,
            SessionContextManager sessionContextManager,
            IOptionsMonitor<ChatSettings> options,
            IOptionsMonitor<PlaybookSettings> settings,
            ILogger<GladosAgent> logger
        )
        {
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
            _agentProfileService = agentProfileService;
            _sessionContextManager = sessionContextManager;
            _settings = options.CurrentValue;
            _playbookSettings = settings.CurrentValue;
            _logger = logger;
        }

        /// <summary>
        /// Create a Playbook from the requirements provided in the
        /// User Prompt
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="currentChatContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ChatHistory> CreatePlaybookAsync(
            VSCodeChatPayloadDto payload,
            CancellationToken cancellationToken = default
        )
        {

            var gladosSettings = _settings.Agents["glados"];
            var playbookHistory = new ChatHistory();

            try
            {
                var gladosAgentPath = gladosSettings.AgentProfilePath;
                var gladosAgent = await _agentProfileService
                    .LoadAgentProfileAsync(gladosAgentPath);

                playbookHistory.AddSystemMessage(gladosAgent.SystemPrompt);
                playbookHistory.AddUserMessage(payload.Prompt);

                var executionSettings = new OllamaPromptExecutionSettings
                {
                    Temperature = gladosAgent.ExecutionSettings.Temperature,
                    TopP = gladosAgent.ExecutionSettings.TopP,
                    FunctionChoiceBehavior = gladosAgent.Permissions.AllowToolCalling
                        ? FunctionChoiceBehavior.Auto()
                        : FunctionChoiceBehavior.None(),
                    ServiceId = gladosAgent.ExecutionSettings.ServiceId,
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };

                var playbookResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    playbookHistory,
                    executionSettings,
                    _kernel,
                    cancellationToken
                );

                if (playbookResponse.Content != null)
                {
                    playbookHistory.AddAssistantMessage(playbookResponse.Content);
                }

                //  Ensure that the prompt is on the Playbook
                playbookHistory.AddUserMessage(payload.Prompt);
                await WritePlaybookAsync(
                    payload,
                    playbookHistory,
                    cancellationToken
                );

                playbookHistory.AddUserMessage(payload.Prompt);
                await WritePlaybookHistoryAsync(
                    payload,
                    playbookHistory,
                    cancellationToken
                );

            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[GLaDOS ERROR]: The payload was invalid.  The cake is a lie.  Error: {ex.Message}");
                Console.WriteLine($"[GLaDOS ERROR]: The payload was invalid.  The cake is a lie.  Error: {ex.Message}");
            }

            return playbookHistory;
        }

        /// <summary>
        /// Write the individual playbook
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="playbookHistory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WritePlaybookAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory playbookHistory,
            CancellationToken cancellationToken = default
        )
        {
            var playbookFilePath = _sessionContextManager
                .GetSummaryFilePath(
                    payload.AgentId,
                    payload.SessionId
                );


            //  Update with correct history path
            playbookFilePath = _playbookSettings.ReasonMCPRootDirectory +
                _playbookSettings.DotReasonDirectory +
                _playbookSettings.PlaybooksDirectory +
                "\\" + payload.SessionId +
                ".GLaDOS.playbook.md";

            var jsonLine = JsonSerializer.Serialize(
                playbookHistory,
                new JsonSerializerOptions
                {
                    WriteIndented = false
                }
            );

            var fileInfo = new FileInfo(playbookFilePath);
            fileInfo.Directory?.Create();

            await File.AppendAllTextAsync(
                playbookFilePath,
                jsonLine + Environment.NewLine,
                cancellationToken
            );
        }

        /// <summary>
        /// Write a running history of playbooks that have been created
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="playbookHistory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WritePlaybookHistoryAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory playbookHistory,
            CancellationToken cancellationToken = default
        )
        {
            var playbookHistoryFilePath = _sessionContextManager
                .GetSummaryFilePath(
                    payload.AgentId,
                    payload.SessionId
                );


            //  Update with correct history path
            playbookHistoryFilePath = _settings.RootDirectory +
                _settings.ChatHistoryDirectory +
                _settings.Agents["glados"].HistoryDirectory +
                _settings.Agents["glados"].HistoryFilename + ".jsonl";

            var jsonLine = JsonSerializer.Serialize(
                playbookHistory,
                new JsonSerializerOptions
                {
                    WriteIndented = false
                }
            );

            var fileInfo = new FileInfo(playbookHistoryFilePath);
            fileInfo.Directory?.Create();

            await File.AppendAllTextAsync(
                playbookHistoryFilePath,
                jsonLine + Environment.NewLine,
                cancellationToken
            );
        }
    }
}