using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace ReasonMCP.Extensions
{
    public static class EmbedServiceExtensions
    {
        public static IHostApplicationBuilder AddReasonNomicEmbedService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Alpaca");

                // Private instance just for embedding
                var ollamaClient = new OllamaApiClient(httpClient) { SelectedModel = "nomic-embed-text:v1.5" };

                return ollamaClient;
            });

            return builder;
        }

    }
}