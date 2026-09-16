using ReasonMCP.Core.Enums;

namespace ReasonMCP.Core.Interfaces
{
    public interface IAgentPermissionEvaluator
    {
        Task<bool> HasPermissionAsync(
            string agentId,
            ResourceType resourceType,
            string resourceTarget,
            CancellationToken cancellationToken
        );
    }
}