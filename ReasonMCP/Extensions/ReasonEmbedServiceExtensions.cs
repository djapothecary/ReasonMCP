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
            this IHostApplicationBuilder builder,
            ILogger logger)
        {
            builder.Services.AddOllamaEmbeddingGenerator(
                modelId: "nomic-embed-text:v1.5",
                endpoint: new Uri("http://localhost:11434") //  TODO:   Refactor: is there a reason to use localhost instead of 127.0.0.1 ?
            );

            builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            {
                var client = sp.GetRequiredService<IOllamaApiClient>();

                //  CRITICAL:   use a new client instance OR ensure the model is switched
                //  in 2026, it is safer to create a seperate client for embeddings
                //  to prevent model-switching race condition
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Alpacca");
                var embedClient = new OllamaApiClient(httpClient) { SelectedModel = "nomic-embed-text:v1.5" };

                //  in OllamaSharp 5.x, the OllamaApiClient natively implements
                //  IEmbeddingGenerator<string, Embedding<float>>.
                //  A direct cast bypasses the broken extension method and its 'struct' constraint
                return embedClient;
            });

            return builder;
        }
    }
}