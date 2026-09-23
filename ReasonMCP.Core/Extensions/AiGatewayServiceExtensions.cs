using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Orchestration;
using ReasonMCP.Core.Services;

namespace ReasonMCP.Core.Extensions
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
            builder.Services.AddScoped<IContextMaintenanceService, ContextMaintenanceService>();

            return builder;
        }
    }
}