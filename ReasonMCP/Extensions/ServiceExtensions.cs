using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Configurations;
using ReasonMCP.Data;
using ReasonMCP.Handlers;
using ReasonMCP.Interfaces;
using ReasonMCP.Services;
using ReasonMCP.Strategies.Converters;
using ReasonMCP.Tools;
using ReasonMCP.Utilities;

namespace ReasonMCP.Extensions
{
    public static class ServiceExtensions
    {
        public static IHostApplicationBuilder AddFileServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.Configure<DocumentsProcessing>(builder.Configuration.GetSection("DocumentProcessing"));
            builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
            builder.Services.AddTransient<IIngestionQueueService, DapperIngestionQueueService>();
            builder.Services.AddTransient<LoggingDelegatingHandler>();
            builder.Services.AddTransient<DocumentContextSearchTool>();
            builder.Services.AddTransient<IMhtmlConverterUtility, MhtmlConverterUtility>();
            builder.Services.AddScoped<IFileConverterUtility, FileConverterUtility>();
            builder.Services.AddScoped<IChunkParsingUtility, ChunkParsingUtility>();
            builder.Services.AddScoped<IMetadataEnrichmentUtility, MetadataEnrichmentUtility>();
            builder.Services.AddScoped<ICodebaseRecordIngestionService, CodebaseRecordIngestionService>();
            builder.Services.AddScoped<IKnowledgebaseRecordIngestionService, KnowledgebaseRecordIngestionService>();
            builder.Services.AddScoped<DapperIngestionQueueService>();

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