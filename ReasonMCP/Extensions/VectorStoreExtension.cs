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
                var store = sp.GetRequiredService<VectorStore>();
                return store.GetCollection<string, CodebaseVectorModel>("Codebase");
            });

            return builder;
        }

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
                var store = sp.GetRequiredService<VectorStore>();
                return store.GetCollection<string, DocumentVectorModel>("Documents");
            });

            return builder;
        }

        public static IHostApplicationBuilder AddReferenceVectorStore(
            this IHostApplicationBuilder builder
        )
        {
            var config = builder.Configuration;

            var dbPath = config.GetValue<string>("StorageConfigSettings:ReferenceDbPath")
                        ?? "reference.db";

            builder.Services.AddSqliteVectorStore(_ => $"Data Source={dbPath}");

            builder.Services.AddSingleton<VectorStoreCollection<string, ReferenceVectorModel>>(sp =>
            {
                var store = sp.GetRequiredService<VectorStore>();
                return store.GetCollection<string, ReferenceVectorModel>("Reference");
            });

            return builder;
        }
    }
}