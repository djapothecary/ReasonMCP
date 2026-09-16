using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Services;

namespace ReasonMCP.Extensions
{
    public static class AiGatewayServiceExtensions
    {
        public static IHostApplicationBuilder AddAiGatewayService(
            this IHostApplicationBuilder builder
        )
        {
            // builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection("ChatSettings"));
            // builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection("GatewaySettings"));

            builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
            builder.Services.AddScoped<SemanticKernelWrapperOrchestrator>();
            builder.Services.AddScoped<IContextMaintenanceService, ContextMaintenanceService>();

            return builder;
        }
    }
}