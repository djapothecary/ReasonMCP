using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Extensions
{
    public static class CodebaseVectorStoreExtensions
    {
        public static IHostApplicationBuilder AddCodebaseVectorStore(
            this IHostApplicationBuilder builder
        )
        {
            var config = builder.Configuration;

            var dbPath = config.GetValue<string>("StorageConfigSettings:CodebaseDbPath")
                        ?? "codebase.db";

            builder.Services.AddSqliteVectorStore(_ => $"Data Source={dbPath}");

            builder.Services.AddSingleton<VectorStoreCollection<string, CodebaseVectorModel>>(sp =>
            {
                var factory = sp.GetRequiredService<ICodebaseDbConnectionFactory>();

                var connection = factory.CreateConnection();

                var isolatedVectorStore = new SqliteVectorStore(connection.ConnectionString);

                return isolatedVectorStore.GetCollection<string, CodebaseVectorModel>("Codebase");
            });

            return builder;
        }
    }
}