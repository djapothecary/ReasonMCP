using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Agents;
using ReasonMCP.Interfaces;
using ReasonMCP.Services;
using ReasonMCP.Strategies.Agents;

namespace ReasonMCP.Extensions
{
    public static class AgentExtensions
    {
        public static IHostApplicationBuilder AddAgentChatStrategies(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<IChatStrategy, BellaChatStrategy>();
            builder.Services.AddScoped<IChatStrategy, ReasonChatStrategy>();
            builder.Services.AddScoped<IChatStrategy, PlanChatStrategy>();

            return builder;
        }

        public static IHostApplicationBuilder AddAgents(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<BellaAgent>();
            builder.Services.AddScoped<PlanAgent>();
            builder.Services.AddScoped<ReasonAgent>();

            return builder;
        }

        public static IHostApplicationBuilder AddAgentServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<CurrentChatContextSummarizer>();

            return builder;
        }
    }
}