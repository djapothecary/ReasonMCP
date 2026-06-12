using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Services;

namespace ReasonMCP.Extensions
{
    public static class OrchestrationExtensions
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
    }
}