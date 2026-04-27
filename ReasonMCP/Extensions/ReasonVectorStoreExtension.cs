using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using ReasonMCP.Models;

namespace ReasonMCP.Extensions
{
    public static class ReasonVectorStoreExtensions
    {
        public static IHostApplicationBuilder AddReasonVectorStore(
            this IHostApplicationBuilder builder
        )
        {
            var config = builder.Configuration;

            var dbPath = config.GetValue<string>("StorageConfig:VectorDbPath")
                        ?? "ReasonContext.db";

            builder.Services.AddSqliteVectorStore(_ => $"Data Source={dbPath}");

            builder.Services.AddSingleton<VectorStoreCollection<string, KnowledgeRecord>>(sp =>
            {
                var store = sp.GetRequiredService<VectorStore>();
                return store.GetCollection<string, KnowledgeRecord>("ReasonContext");
            });

            return builder;
        }
    }
}