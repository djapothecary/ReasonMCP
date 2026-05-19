using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using FunctionInvocationContext = Microsoft.SemanticKernel.FunctionInvocationContext;

namespace ReasonMCP.Filters
{
    /// <summary>
    /// Intercepts Semantic Kernel AI invocations to enable Context Compression,
    /// LLM-as-a-Judge grading, and prompt reinforcement.
    /// </summary>
    public class ReasonChatInterceptor : IFunctionInvocationFilter
    {
        private readonly ILogger<ReasonChatInterceptor> _logger;

        public ReasonChatInterceptor(
            ILogger<ReasonChatInterceptor> logger
        )
        {
            _logger = logger;
        }

        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next
        )
        {
            // ==========================================
            // PRE-FLIGHT: Intercepting the Prompt
            // ==========================================

            // In the future, this is where you will check the ChatHistory length
            // and perform the Context Compression swap before it goes to Ollama.
            _logger.LogInformation("\n[PROMPT INTERCEPTED] -> Sending context to Reason.");
            Console.WriteLine("[PROMPT INTERCEPTED] -> Sending context to Reason.");

            // Optionally, you can inspect the arguments being sent to the model:
            // if (context.Arguments.TryGetValue("input", out var userInput))
            // {
            //     _logger.LogTrace("User Input: {Input}", userInput);
            // }

            // ==========================================
            // EXECUTION: Call the LLM
            // ==========================================

            await next(context); // This actually fires the request to 127.0.0.1:11434

            // ==========================================
            // POST-FLIGHT: Intercepting the Response
            // ==========================================

            // In the future, this is where you grab context.Result, send it to your
            // "LLM-as-a-Judge" for grading, and append the reinforcement string.
            _logger.LogInformation("[RESPONSE REGISTERED] <- Received completion from Reason.\n");
            Console.WriteLine("[RESPONSE REGISTERED] <- Received completion from Reason.");

            // Example of how you will eventually read the result:
            // var rawResponse = context.Result?.ToString();

        }
    }
}