using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces;
using ReasonMCP.Processors;


namespace ReasonMCP.Extensions
{
    public static class ReasonAstChunkingServiceExtensions
    {
        public static IHostApplicationBuilder AddAstChunkingService(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<ICodeChunkingStrategy, CSharpRoslynChunkingStrategy>();
            builder.Services.AddScoped<ICodeChunkingStrategy, TypeScriptChunkingStrategy>();

            return builder;
        }
    }
}