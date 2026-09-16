using ReasonMCP.Enums;

namespace ReasonMCP.Interfaces
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