using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Services;

namespace ReasonMCP.Agents.Agents
{
    public class MnemosyneAgent : IMnemosyneAgent
    {
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IAgentProfileService _agentProfileService;
        private readonly SessionContextManager _sessionContextManager;
        private readonly ChatSettings _settings;
        private readonly ILogger<MnemosyneAgent> _logger;

        public MnemosyneAgent
        (
            Kernel kernel,
            IServiceProvider serviceProvider,
            [FromKeyedServices("MnemosyneService")] IChatCompletionService chatCompletionService,
            IAgentProfileService agentProfileService,
            SessionContextManager sessionContextManager,
            IOptionsMonitor<ChatSettings> options,
            ILogger<MnemosyneAgent> logger
        )
        {
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
            _agentProfileService = agentProfileService;
            _sessionContextManager = sessionContextManager;
            _settings = options.CurrentValue;
            _logger = logger;
        }

        public async Task<ChatHistory> CreateSummaryAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory currentChatContext,
            CancellationToken cancellationToken = default
        )
        {
            var mnemosyneSettings = _settings.Agents["mnemosyne"];
            var summaryHistory = new ChatHistory();

            try
            {
                //  We know that this is Mneomsyne so get the path and load the file
                var mnemosyneAgentPath = mnemosyneSettings.AgentProfilePath;
                var mnemosyneAgent = await _agentProfileService.LoadAgentProfileAsync(mnemosyneAgentPath);

                currentChatContext.AddSystemMessage(mnemosyneAgent.SystemPrompt);
                currentChatContext.AddUserMessage(payload.Prompt);

                var executionSettings = new OllamaPromptExecutionSettings
                {
                    Temperature = mnemosyneAgent.ExecutionSettings.Temperature,
                    TopP = mnemosyneAgent.ExecutionSettings.TopP,
                    FunctionChoiceBehavior = mnemosyneAgent.Permissions.AllowToolCalling
                        ? FunctionChoiceBehavior.Auto()
                        : FunctionChoiceBehavior.None(),
                    ServiceId = mnemosyneAgent.ExecutionSettings.ServiceId,
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };

                var summaryResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    currentChatContext,
                    executionSettings,
                    _kernel,
                    cancellationToken
                );

                if (summaryResponse.Content != null)
                {
                    summaryHistory.AddAssistantMessage(summaryResponse.Content);
                }

                //  Ensure the prompt is on the new summary
                summaryHistory.AddUserMessage(payload.Prompt);
                await WriteSummaryAsync(
                    payload,
                    summaryHistory,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {

            }

            return summaryHistory;
        }

        public async Task WriteSummaryAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory summaryHistory,
            CancellationToken cancellationToken = default
        )
        {
            var summaryFilePath = _sessionContextManager
                .GetSummaryFilePath(
                    payload.AgentId,
                    payload.SessionId
                );

            //  update with correct history path
            summaryFilePath = _settings.RootDirectory +
                _settings.ChatHistoryDirectory +
                _settings.Agents["mnemosyne"].HistoryDirectory +
                _settings.Agents["mnemosyne"].HistoryFilename + ".jsonl";

            var jsonLine = JsonSerializer.Serialize(
                summaryHistory,
                new JsonSerializerOptions
                {
                    WriteIndented = false
                }
            );

            var fileInfo = new FileInfo(summaryFilePath);
            fileInfo.Directory?.Create();

            await File.AppendAllTextAsync(
                summaryFilePath,
                jsonLine + Environment.NewLine,
                cancellationToken
            );
        }
    }

}