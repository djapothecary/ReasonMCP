using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReasonMCP.Data;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Extensions
{
    public static class IngestionQueueDbExtension
    {
        public static IHostApplicationBuilder AddIngestionQueueService(
            this IHostApplicationBuilder builder
        )
        {
            // 1. Register Database Initializer
            builder.Services.AddTransient<IngestionQueueDbInitializer>();

            // 2. Register the Factory (Clean, one-liner!)
            builder.Services.AddSingleton<IIngestionQueueDbConnectionFactory, IngestionQueueDbConnectionFactory>();

            return builder;
        }
    }
}