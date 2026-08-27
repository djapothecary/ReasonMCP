using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Configurations;
using ReasonMCP.Records;
using ReasonMCP.Tools;
using ReasonMCP.Utilities;

namespace ReasonMCP.Agents
{
    public class BellaAgent
    {
        private readonly Channel<dynamic> _agentTaskChannel;
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ChatSettings _settings;
        private readonly ILogger<BellaAgent> _logger;

        public BellaAgent
        (
            Channel<dynamic> agentTaskChannel,
            Kernel kernel,
            IServiceProvider serviceProvider,
            [FromKeyedServices("Reason")] IChatCompletionService chatCompletionService,
            IOptionsMonitor<ChatSettings> options,
            ILogger<BellaAgent> logger
        )
        {
            _agentTaskChannel = agentTaskChannel;
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
            _settings = options.CurrentValue;
            _logger = logger;
        }

        /// <summary>
        /// Sends the completed prompt (and attachments if there are any)
        /// to the Bella Agent
        /// </summary>
        /// <param name="agentProfile"></param>
        /// <param name="currentContext"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public async Task<List<ChatMessageRecord>> SendPrompt(
            AgentProfile agentProfile,
            ChatHistory currentContext,
            string prompt,
            string currentContextFilePath
        )
        {
            var agentResponseChatMessageRecord = new List<ChatMessageRecord>();
            try
            {
                var bellaSettings = _settings.Agents["bella"];

                //  1.  Get available tools
                var codebaseSearchTool = _serviceProvider
                    .GetRequiredService<CodebaseContextSearchTool>();

                var documentSearchTool = _serviceProvider
                    .GetRequiredService<DocumentContextSearchTool>();

                var referenceSearchTool = _serviceProvider
                    .GetRequiredService<ReferenceContextSearchTool>();

                var randomNumberTool = _serviceProvider
                .GetRequiredService<RandomNumberTools>();

                //  2.  Inject tools into the kernel
                _kernel.Plugins.AddFromObject(codebaseSearchTool, "CodebaseSearch");
                _kernel.Plugins.AddFromObject(documentSearchTool, "DocumentSearch");
                _kernel.Plugins.AddFromObject(referenceSearchTool, "ReferenceSerach");
                _kernel.Plugins.AddFromObject(randomNumberTool, "RandomNumbers");

                currentContext.AddUserMessage(prompt);
                currentContext.AddSystemMessage(agentProfile.SystemPrompt);

                var executionSettings = new OllamaPromptExecutionSettings
                {
                    Temperature = agentProfile.ExecutionSettings.Temperature,
                    TopP = agentProfile.ExecutionSettings.TopP,
                    FunctionChoiceBehavior = agentProfile.Permissions.AllowToolCalling
                        ? FunctionChoiceBehavior.Auto()
                        : FunctionChoiceBehavior.None(),
                    ServiceId = agentProfile.ExecutionSettings.ServiceId,   //  This refers to the named connection "Alpaca" or "Vicuna" (used for grading)
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };


                var agentResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    currentContext,
                    executionSettings,
                    _kernel
                );

                var cleandedAgentResponse = ProcessJsonResponseUtility.TryParseChatMessageContent(agentResponse);

                agentResponseChatMessageRecord.Add(
                    new ChatMessageRecord("assistant",
                        cleandedAgentResponse
                        ?? "Bella's chasing squirrels ..."));

                var turnCount = currentContext.Count(m => m.Role == AuthorRole.User);
                if (turnCount > _settings.ChatSummarizationThreshold)
                {
                    _agentTaskChannel.Writer.TryWrite(new AgentTask(
                        "PlayFetch",
                        currentContextFilePath
                    ));
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return agentResponseChatMessageRecord;
        }
    }
}