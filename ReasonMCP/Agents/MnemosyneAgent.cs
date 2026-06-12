using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Configurations;
using ReasonMCP.DTOs;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Services;
using ReasonMCP.Tools;

namespace ReasonMCP.Agents
{
    public class MnemosyneAgent : IMnemosyne
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
            IOptions<ChatSettings> options,
            ILogger<MnemosyneAgent> logger
        )
        {
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
            _agentProfileService = agentProfileService;
            _sessionContextManager = sessionContextManager;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<ChatHistory> CreateSummary(
            VSCodeChatPayloadDto payload,
            ChatHistory currentChatContext
        )
        {
            var mnemosyneSettings = _settings.Agents["mnemosyne"];
            var summaryHistory = new ChatHistory();

            try
            {
                //  We know that this is Mneomsyne so get the path and load the file
                var mnemosyneAgentPath = mnemosyneSettings.AgentProfilePath;
                var mnemosyneAgent = await _agentProfileService.LoadAgentProfileAsync(mnemosyneAgentPath);

                currentChatContext.AddUserMessage(payload.Prompt);
                currentChatContext.AddSystemMessage(mnemosyneAgent.SystemPrompt);

                var executionSettings = new OllamaPromptExecutionSettings
                {
                    Temperature = mnemosyneAgent.ExecutionSettings.Temperature,
                    TopP = mnemosyneAgent.ExecutionSettings.TopP,
                    FunctionChoiceBehavior = mnemosyneAgent.Permissions.AllowToolCalling
                        ? FunctionChoiceBehavior.Auto()
                        : FunctionChoiceBehavior.None(),
                    ServiceId = mnemosyneAgent.ExecutionSettings.ServiceId, //  This is "Vicuna" because we want to keep it on the different model
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };

                var summaryResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    currentChatContext,
                    executionSettings,
                    _kernel
                );

                if (summaryResponse.Content != null)
                {
                    summaryHistory.AddAssistantMessage(summaryResponse.Content);
                }

                //  Ensure the prompt is on the new summary
                summaryHistory.AddUserMessage(payload.Prompt);
            }
            catch (Exception ex)
            {

            }

            return summaryHistory;
        }

        public async Task WriteSummary(
            VSCodeChatPayloadDto payload,
            ChatHistory summaryHistory
        )
        {
            var summaryFilepath = _sessionContextManager.GetSummaryFilePath(payload.AgentId, payload.SessionId);

            var jsonLine = JsonSerializer.Serialize(summaryHistory, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            var fileInfo = new FileInfo(summaryFilepath);
            fileInfo.Directory?.Create();

            await File.AppendAllTextAsync(summaryFilepath, jsonLine + Environment.NewLine);
        }
    }

}