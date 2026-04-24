using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OllamaSharp;
using ReasonMCP.Handlers;

namespace ReasonMCP.Extensions
{
    public static class ReasonServiceExtensions
    {
        public static IHostApplicationBuilder AddReasonOllamaService(
            this IHostApplicationBuilder builder,
            ILogger logger
        )
        {
            builder.Services.AddHttpClient("Alpacca", client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:11434");
                client.TimeOut = TimeSpan.FromMinutes(5);
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .RemoveAllLoggers()
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
                //  Sample duration must be at least twice the attempt timeout
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
                options.CircuitBreaker.FAilureRatio = 0.9;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.Retry.MaxRetryAttempts = 1;
            });

            return builder;
        }
    }
}