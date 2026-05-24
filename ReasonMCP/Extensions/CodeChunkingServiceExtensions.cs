using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Processors;


namespace ReasonMCP.Extensions
{
    public static class CodeChunkingServiceExtensions
    {
        public static IHostApplicationBuilder AddCodeChunkingServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<ConfigChunkingProcessor>();
            builder.Services.AddScoped<MarkupChunkingProcessor>();
            builder.Services.AddScoped<CodebaseRecordUpsertOrchestrator>();
            builder.Services.AddScoped<ICodeChunkingProcessor, CSharpRoslynChunkingProcessor>();
            builder.Services.AddScoped<ICodeChunkingProcessor, TypeScriptChunkingProcessor>();

            return builder;
        }
    }
}