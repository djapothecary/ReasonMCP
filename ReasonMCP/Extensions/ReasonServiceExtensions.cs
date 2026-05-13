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
            builder.Services.AddScoped<IFileConverterUtility, FileConverterUtility>();
            builder.Services.AddScoped<IChunkParsingUtility, ChunkParsingUtility>();
            builder.Services.AddScoped<IMetadataEnrichmentUtility, MetadataEnrichmentUtility>();
            builder.Services.AddScoped<IFileIngestionService, FileIngestionService>();

            return builder;
        }

        public static IHostApplicationBuilder AddOrchestrators(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<CodebaseScanOrchestrator>();
            builder.Services.AddScoped<FileUpsertOrchestrator>();
            builder.Services.AddScoped<PreProcessOrchestrator>();

            return builder;
        }

        public static IHostApplicationBuilder AddStrategies(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<IFileConverterStrategy, ConfigConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, MarkdownFileStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, MarkupConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, MhtmlConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, PdfConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, SourceCodeConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, SqlScriptConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, TxtConverterStrategy>();

            return builder;
        }
    }
}