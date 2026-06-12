using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using ReasonMCP.Handlers;
using ReasonMCP.Tools;

namespace ReasonMCP.Extensions
{
    public static class AIServiceExtensions
    {
        public static IHostApplicationBuilder AddReasonOllamaService(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddHttpClient("Alpaca", client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:11434");
                client.Timeout = TimeSpan.FromMinutes(5);
            })
            .RemoveAllLoggers()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
                //  Sample duration must be at least twice the attempt timeout
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
                options.CircuitBreaker.FailureRatio = 0.9;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.Retry.MaxRetryAttempts = 1;
            });

            return builder;
        }

        public static IHostApplicationBuilder AddChatCompletionService(this IHostApplicationBuilder builder)
        {
            // We instantiate OllamaApiClient INLINE, directly returning the IChatCompletionService.
            builder.Services.AddKeyedSingleton<IChatCompletionService>("Reason", (sp, key) =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Alpaca");

                // Private instance just for chat
                var ollamaClient = new OllamaApiClient(httpClient) { SelectedModel = "Reason" };

                IChatClient chatClient = new ChatClientBuilder(ollamaClient)
                    .UseFunctionInvocation()
                    .Build();

                return chatClient.AsChatCompletionService();
            });

            return builder;
        }

        public static IHostApplicationBuilder AddMnemosyneSummaryService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddKeyedSingleton<IChatCompletionService>("MnemosyneService", (sp, key) =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Alpaca");

                var ollamaClient = new OllamaApiClient(httpClient) { SelectedModel = "Mnemosyne" };

                IChatClient chatClient = new ChatClientBuilder(ollamaClient)
                    .Build();

                return chatClient.AsChatCompletionService();
            });

            return builder;
        }

        public static IHostApplicationBuilder AddAIPluginsAndTools(
            this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<DocumentContextSearchTool>();
            builder.Services.AddSingleton<RandomNumberTools>();

            return builder;
        }
    }
}