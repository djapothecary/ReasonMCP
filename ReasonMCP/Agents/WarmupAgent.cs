using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Records;

namespace ReasonMCP.Agents
{
    public class WarmupAgent
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<WarmupAgent> _logger;

        public WarmupAgent
        (
            Kernel kernel,
            IChatCompletionService chatCompletionService,
            ILogger<WarmupAgent> logger
        )
        {
            _kernel = kernel;
            _chatCompletionService = chatCompletionService;
            _logger = logger;
        }

        public async Task Wakeup()
        {
            try
            {
                var executionSettings = new OllamaPromptExecutionSettings
                {
                    // Temperature = 0.7f,
                    Temperature = 0f,
                    ServiceId = "Alpaca",
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };


                // Create a tiny, microscopic prompt just to force Ollama to load the weights into memory
                var messages = new[]
                    {
                    // "Hello, this is a warm-up response!",
                    // "Starting up... all systems go.",
                    "Wakey-wakey, let's code together!",
                    //"It looks like I'm ready for our chat session."
                };
                var warmupHistory = new ChatHistory();
                foreach (var msg in messages)
                {
                    warmupHistory.AddAssistantMessage(msg);
                    warmupHistory.AddUserMessage("Ping");

                    // Fire and wait for the model to load into memory
                    await _chatCompletionService.GetChatMessageContentAsync(warmupHistory);

                }

                _logger.LogInformation("Reason LLM is awake and loaded into VRAM.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Pre-warming LLM failed, first user prompt may be slow. Error: {ex.Message}");
            }

            _logger.LogInformation("Ingestion complete. Start MCP Server loop ...");
        }

    }
}