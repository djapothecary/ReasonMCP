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
            builder.Services.AddTransient<CodebaseScanOrchestrator>();
            builder.Services.AddTransient<KnowledgebaseScanOrchestrator>();
            builder.Services.AddTransient<CodebaseRecordUpsertOrchestrator>();
            builder.Services.AddTransient<KnowledgebaseRecordUpsertOrchestrator>();
            builder.Services.AddTransient<PreProcessOrchestrator>();

            return builder;
        }

        public static IHostApplicationBuilder AddCodeChunkingServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<ConfigChunkingProcessor>();
            builder.Services.AddTransient<CSharpRoslynChunkingProcessor>();
            builder.Services.AddTransient<MarkupChunkingProcessor>();
            builder.Services.AddTransient<SqlScriptChunkingProcessor>();
            builder.Services.AddTransient<TypeScriptChunkingProcessor>();

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