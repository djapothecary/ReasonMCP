using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReaconMCP.Interfaces;
using ReasonMCP.Handlers;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Processors;
using ReasonMCP.Services;
using ReasonMCP.Tools;
using ReasonMCP.Utilities;

namespace ReasonMCP.Extensions
{
    public static class ReasonServiceExtensions
    {
        public static IHostApplicationBuilder AddFileServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<LoggingDelegatingHandler>();
            builder.Services.AddTransient<KnowledgeSearchTool>();
            builder.Services.AddTransient<IMhtmlConverterUtility, MhtmlConverterUtility>();
            builder.Services.AddScoped<PreProcessOrchestrator>();
            builder.Services.AddScoped<FileUpsertOrchestrator>();
            builder.Services.AddScoped<IFileConverterStrategy, TxtConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, MhtmlConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, PdfConverterStrategy>();
            builder.Services.AddScoped<IFileConverterUtility, FileConverterUtility>();
            builder.Services.AddScoped<IChunkParsingUtility, ChunkParsingUtility>();
            builder.Services.AddScoped<IMetadataEnrichmentUtility, MetadataEnrichmentUtility>();
            builder.Services.AddScoped<IFileIngestionService, FileIngestionService>();

            return builder;
        }
    }
}