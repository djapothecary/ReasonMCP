using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Processors;
using ReasonMCP.Services;


namespace ReasonMCP.Extensions
{
    public static class CodeChunkingServiceExtensions
    {

        public static IHostApplicationBuilder AddOrchestrators(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<CodebaseScanOrchestrator>();
            builder.Services.AddScoped<KnowledgebaseScanOrchestrator>();
            builder.Services.AddScoped<CodebaseRecordUpsertOrchestrator>();
            builder.Services.AddScoped<KnowledgebaseRecordUpsertOrchestrator>();
            builder.Services.AddScoped<PreProcessOrchestrator>();

            return builder;
        }

        public static IHostApplicationBuilder AddCodeChunkingServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<ICodeChunkingProcessor, ConfigChunkingProcessor>();
            builder.Services.AddScoped<ICodeChunkingProcessor, MarkupChunkingProcessor>();
            builder.Services.AddScoped<ICodeChunkingProcessor, SqlScriptChunkingProcessor>();
            builder.Services.AddScoped<ICodeChunkingProcessor, TypeScriptChunkingProcessor>();
            builder.Services.AddScoped<ICSharpRoslynChunkingProcessor, CSharpRoslynChunkingProcessor>();

            return builder;
        }
        public static IHostApplicationBuilder AddIngestionQueueServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<IIngestionQueueUpdaterService, IngestionQueueUpdaterService>();

            return builder;
        }
    }
}