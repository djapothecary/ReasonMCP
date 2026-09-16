using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Data;
using ReasonMCP.Core.Handlers;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Services;
using ReasonMCP.Core.Utilities;

namespace ReasonMCP.Core.Extensions
{
    public static class ServiceExtensions
    {
        public static IHostApplicationBuilder AddFileServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.Configure<DocumentScanSettings>(builder.Configuration.GetSection("DocumentScanSettings"));
            builder.Services.AddTransient<IIngestionQueueService, DapperIngestionQueueService>();
            builder.Services.AddTransient<LoggingDelegatingHandler>();
            builder.Services.AddTransient<DocumentContextSearchTool>();
            builder.Services.AddTransient<IMhtmlConverterUtility, MhtmlConverterUtility>();
            builder.Services.AddScoped<IFileConverterUtility, FileConverterUtility>();
            builder.Services.AddScoped<IFileSystemSecurityService, FileSystemSecurityService>();
            builder.Services.AddScoped<ILocalFileSystemService, LocalFileSystemService>();
            builder.Services.AddScoped<IChunkParsingUtility, ChunkParsingUtility>();
            builder.Services.AddScoped<IMetadataEnrichmentUtility, MetadataEnrichmentUtility>();
            builder.Services.AddScoped<DapperIngestionQueueService>();
            builder.Services.AddScoped<SessionContextManager>();

            return builder;
        }
    }
}