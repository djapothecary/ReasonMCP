using System.Security.AccessControl;
using Microsoft.SemanticKernel;
using ReasonMCP.Core.Enums;
using ReasonMCP.Core.Interfaces;
using ResourceType = ReasonMCP.Core.Enums.ResourceType;

namespace ReasonMCP.Core.Services
{
    public class AgentAuthorizationFilter : IFunctionInvocationFilter
    {
        private readonly IAgentPermissionEvaluator _permissionEvaluator;
        private readonly string _currentAgentId;   //  injected per-request via HttpContext/Extension state

        public AgentAuthorizationFilter(
            IAgentPermissionEvaluator permissionEvaluator,
            string currentAgentId
        )
        {
            _permissionEvaluator = permissionEvaluator;
            _currentAgentId = currentAgentId;
        }

        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next
        )
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            string pluginName = context.Function.PluginName;
            string functionName = context.Function.Name;

            //  Evaluate if the AgentId has access to "PluginName.FunctionName"
            bool isAuthorized = await _permissionEvaluator.HasPermissionAsync(
                _currentAgentId,
                ResourceType.Plugin,
                $"{pluginName}.{functionName}",
                cancellationToken
            );

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException($"Agent '{_currentAgentId}' is not authorized to execute {pluginName}.{functionName}.");
            }

            await next(context);
        }
    }
}