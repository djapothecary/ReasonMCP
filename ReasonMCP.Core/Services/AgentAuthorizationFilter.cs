using System.Security.AccessControl;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ReasonMCP.Core.Enums;
using ReasonMCP.Core.Interfaces;
using ResourceType = ReasonMCP.Core.Enums.ResourceType;

namespace ReasonMCP.Core.Services
{
    public class AgentAuthorizationFilter : IFunctionInvocationFilter
    {
        private readonly IAgentPermissionEvaluator _permissionEvaluator;
        private readonly IServiceScopeFactory _scopeFactory;

        public AgentAuthorizationFilter(
            IAgentPermissionEvaluator permissionEvaluator,
            IServiceScopeFactory scopeFactory
        )
        {
            _permissionEvaluator = permissionEvaluator;
            _scopeFactory = scopeFactory;
        }

        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next
        )
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            string currentAgentId = context.Arguments["agentId"]?.ToString() ?? string.Empty;

            string pluginName = context.Function.PluginName;
            string functionName = context.Function.Name;

            //  Evaluate if the AgentId has access to "PluginName.FunctionName"
            bool isAuthorized = await _permissionEvaluator.HasPermissionAsync(
                currentAgentId,
                ResourceType.Plugin,
                $"{pluginName}.{functionName}",
                cancellationToken
            );

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException($"Agent '{currentAgentId}' is not authorized to execute {pluginName}.{functionName}.");
            }

            await next(context);
        }
    }
}