using ReasonMCP.Core.Enums;

namespace ReasonMCP.Core.Models
{
    public class AgentPermission
    {
        public string AgentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // e.g., "ExecutePlugin"

        //  Semantic Kernel specific Targets
        public ResourceType ResourceType { get; set; }
        public string ResourceTarget { get; set; } = string.Empty; //   e.g., "CodebaseContextSearch"
        public AccessEffect Effect { get; set; } = AccessEffect.Allow; //   Allow or Deny

    }
}