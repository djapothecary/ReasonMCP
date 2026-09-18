using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReasonMCP.Core.Data;
using ReasonMCP.Core.Interfaces;

namespace ReasonMCP.Core.Extensions
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