using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Data;
using ReasonMCP.Core.Handlers;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Orchestration;
using ReasonMCP.Core.Services;
using ReasonMCP.Core.Utilities;
using ReasonMCP.Core.Workflows;

namespace ReasonMCP.Core.Extensions
{
    public static class PlaybookExtensions
    {
        public static IHostApplicationBuilder AddOrchestrators(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<SemanticKernelWrapperOrchestrator>();
            builder.Services.AddScoped<MozzieFileOrchestrator>();
            builder.Services.AddScoped<AgenticPlaybookOrchestrator>();

            return builder;
        }

        public static IHostApplicationBuilder AddAgenticPlaybookServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<IAgenticPlaybookService, AgenticPlaybookService>();
            builder.Services.AddScoped<IPlaybookWorkflow, PlaybookWorkflow>();

            return builder;
        }
    }
}