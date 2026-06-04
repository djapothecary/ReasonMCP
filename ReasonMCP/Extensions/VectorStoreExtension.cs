using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Models;

namespace ReasonMCP.Extensions
{
    public static class VectorStoreExtensions
    {
        public static IHostApplicationBuilder AddReasonVectorStore(
            this IHostApplicationBuilder builder
        )
        {
            var config = builder.Configuration;

            var dbPath = config.GetValue<string>("StorageConfigSettings:VectorDbPath")
                        ?? "ReasonContext.db";

            builder.Services.AddSqliteVectorStore(_ => $"Data Source={dbPath}");

            builder.Services.AddSingleton<VectorStoreCollection<string, KnowledgebaseEntity>>(sp =>
            {
                var store = sp.GetRequiredService<VectorStore>();
                return store.GetCollection<string, KnowledgebaseEntity>("DocumentsContext");
            });

            builder.Services.AddSingleton<VectorStoreCollection<string, CodebaseEntity>>(sp =>
            {
                var store = sp.GetRequiredService<VectorStore>();
                return store.GetCollection<string, CodebaseEntity>("CodebaseContext");
            });

            return builder;
        }

        public static IHostApplicationBuilder AddVectorContexts(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddSingleton(sp =>
            {
                var vectorStore = sp.GetRequiredService<VectorStore>();
                return vectorStore.GetCollection<string, KnowledgebaseEntity>("DocumentsContext");
            });

            builder.Services.AddSingleton(sp =>
            {
                var vectorStore = sp.GetRequiredService<VectorStore>();
                return vectorStore.GetCollection<string, CodebaseEntity>("CodebaseContext");
            });

            return builder;
        }
    }
}