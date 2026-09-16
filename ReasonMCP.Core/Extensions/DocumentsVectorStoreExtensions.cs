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
    public static class DocumentsVectorStoreExtensions
    {
        public static IHostApplicationBuilder AddDocumentsVectorStore(
            this IHostApplicationBuilder builder
        )
        {
            var config = builder.Configuration;

            var dbPath = config.GetValue<string>("StorageConfigSettings:DocumentsDbPath")
                        ?? "documents.db";

            builder.Services.AddSqliteVectorStore(_ => $"Data Source={dbPath}");

            builder.Services.AddSingleton<VectorStoreCollection<string, DocumentVectorModel>>(sp =>
            {
                var factory = sp.GetRequiredService<IDocumentsDbConnectionFactory>();

                var connection = factory.CreateConnection();

                var isolatedVectorStore = new SqliteVectorStore(connection.ConnectionString);

                return isolatedVectorStore.GetCollection<string, DocumentVectorModel>("Documents");
            });

            return builder;
        }
    }
}