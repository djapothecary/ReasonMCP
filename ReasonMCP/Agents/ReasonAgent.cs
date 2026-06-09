using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Configurations;
using ReasonMCP.Records;
using ReasonMCP.Tools;

namespace ReasonMCP.Agents
{
    public class ReasonAgent
    {
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<ReasonAgent> _logger;

        public ReasonAgent
        (
            Kernel kernel,
            IServiceProvider serviceProvider,
            IChatCompletionService chatCompletionService,
            ILogger<ReasonAgent> logger
        )
        {
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
            _logger = logger;
        }

        /// <summary>
        /// Sends the completed prompt (and attachments if there are any)
        /// to the Reason Agent
        /// </summary>
        /// <param name="agentProfile"></param>
        /// <param name="currentContext"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public async Task<List<ChatMessageRecord>> SendPrompt(
            AgentProfile agentProfile,
            ChatHistory currentContext,
            string prompt
        )
        {
            var agentResponseChatMessageRecord = new List<ChatMessageRecord>();
            try
            {
                //  1.  Get available tools
                var documentSearchtool = _serviceProvider.GetRequiredService<DocumentContextSearchTool>();
                var randomNumberTool = _serviceProvider.GetRequiredService<RandomNumberTools>();

                //  2.  Inject tools into the kernel
                _kernel.Plugins.AddFromObject(documentSearchtool, "DocumentSearch");
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
                    ServiceId = agentProfile.ExecutionSettings.ServiceId,
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };

                var agentResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    currentContext,
                    executionSettings,
                    _kernel
                );

                agentResponseChatMessageRecord.Add(new ChatMessageRecord("assistant", agentResponse.Content ?? "Reason took SnowCrash.  Friend's don't let friends do SnowCrash ..."));
            }
            catch (System.Exception)
            {

                throw;
            }

            return agentResponseChatMessageRecord;
        }
    }
}