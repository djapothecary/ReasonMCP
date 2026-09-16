using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Agents.Agents;
using ReasonMCP.Agents.Interfaces;
using ReasonMCP.Agents.Strategies;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Services;
using ReasonMPC.Agents;

namespace ReasonMCP.Agents.Extensions
{
    public static class AgentExtensions
    {
        public static IHostApplicationBuilder AddAgentChatStrategies(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection("ChatSettings"));
            builder.Services.AddScoped<IChatStrategy, BellaAgentStrategy>();
            builder.Services.AddScoped<IChatStrategy, DozerAgentStrategy>();
            builder.Services.AddScoped<IChatStrategy, MozzieAgentStrategy>();
            builder.Services.AddScoped<IChatStrategy, ReasonAgentStrategy>();
            builder.Services.AddScoped<IChatStrategy, SeraphAgentStrategy>();
            builder.Services.AddScoped<IChatStrategy, TankAgentStrategy>();

            return builder;
        }

        public static IHostApplicationBuilder AddAgents(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<WarmupAgent>();
            builder.Services.AddScoped<BellaAgent>();
            builder.Services.AddScoped<DozerAgent>();
            builder.Services.AddScoped<MozzieAgent>();
            builder.Services.AddScoped<ReasonAgent>();
            builder.Services.AddScoped<SeraphAgent>();
            builder.Services.AddScoped<TankAgent>();

            return builder;
        }

        public static IHostApplicationBuilder AddAgentServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<ChatHistoryService>();
            builder.Services.AddScoped<CurrentChatContextSummarizer>();
            builder.Services.AddScoped<IAgentProfileService, AgentProfileService>();
            builder.Services.AddScoped<IMnemosyne, MnemosyneAgent>();

            return builder;
        }
    }
}