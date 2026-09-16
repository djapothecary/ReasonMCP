using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces;
using ReasonMCP.Services;

namespace ReasonMCP.Extensions
{
    public static class PermissionPoliciesExtensions
    {
        public static IHostApplicationBuilder AddAgentPermissonPolicies(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddSingleton<AgentAuthorizationFilter>();
            builder.Services.AddSingleton<IAgentPermissionEvaluator, AgentPermissionEvaluator>();

            return builder;
        }
    }
}