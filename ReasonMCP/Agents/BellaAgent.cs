using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Tools;
using SQLitePCL;

namespace ReasonMCP.Agents
{
    public class BellaAgent
    {
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<BellaAgent> _logger;

        public BellaAgent
        (
            Kernel kernel,
            IServiceProvider serviceProvider,
            IChatCompletionService chatCompletionService,
            ILogger<BellaAgent> logger
        )
        {
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
            _logger = logger;
        }

        public async Task<List<ChatMessageRecord>> SendPrompt(
            AgentProfile agentProfile,
            ChatHistory currentContext,
            string prompt
        )
        {
            var agentresponseChatMessageRecord = new List<ChatMessageRecord>();
            try
            {
                //  1.  Get available tools
                var searchTool = _serviceProvider.GetRequiredService<DocumentContextSearchTool>();
                var randomNumberTool = _serviceProvider.GetRequiredService<RandomNumberTools>();

                //  2.  Inject ools into the kernel
                _kernel.Plugins.AddFromObject(searchTool, "DocumentSearch");
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

                agentresponseChatMessageRecord.Add(new ChatMessageRecord("assistant", agentResponse.Content ?? "Bella's chasing squirrels ..."));
            }
            catch (System.Exception)
            {

                throw;
            }

            return agentresponseChatMessageRecord;
        }

    }
}