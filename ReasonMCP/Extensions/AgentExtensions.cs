using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces;
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
    }
}