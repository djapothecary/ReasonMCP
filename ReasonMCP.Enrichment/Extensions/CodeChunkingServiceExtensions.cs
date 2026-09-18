using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Enrichment.Interfaces;
using ReasonMCP.Enrichment.Processors;


namespace ReasonMCP.Enrichment.Extensions
{
    public static class CodeChunkingServiceExtensions
    {
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
    }
}