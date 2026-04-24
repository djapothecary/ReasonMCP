using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace ReasonMCP.Extensions
{
    public static class ReasonEmbedServiceExtensions
    {
        public static IHostApplicationBuilder AddReasonNomicEmbedService(
            this IHostApplicationBuilder builder
        )
        {
            // Register the OllamaApiClient
            builder.Services.AddSingleton<IOllamaApiClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Ollama");
                httpClient.Timeout = TimeSpan.FromSeconds(300); // Changed from TimeOut to Timeout
                return new OllamaApiClient(httpClient) { SelectedModel = "nomic-embed-text:v1.5" };
            });

            // Register the embedding generator
            builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            {
                var embedClient = sp.GetRequiredService<IOllamaApiClient>();
                return (IEmbeddingGenerator<string, Embedding<float>>)embedClient;
            });

            return builder;
        }
    }
}