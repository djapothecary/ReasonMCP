using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces;
using ReasonMCP.Services;

namespace ReasonMCP.Extensions
{
    public static class ReasonAstChunkingServiceExtensions
    {
        public static IHostApplicationBuilder AddAstChunkingService(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddSingleton<IAstChunkingService, RoslynAstChunkingService>();

            return builder;
        }
    }
}