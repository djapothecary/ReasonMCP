using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Services;

namespace ReasonMCP.Core.Extensions
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