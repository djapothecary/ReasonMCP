namespace ReasonMCP.Core.Models
{
    public class AgentRoleAssignment
    {
        public string AgentId { get; set; } = string.Empty;
        public AgentIdentity Agent { get; set; } = null!;
        public string RoleId { get; set; } = string.Empty;
        public AgentRoleAssignment Role { get; set; } = null!;
    }
}