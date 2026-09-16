namespace ReasonMCP.Models
{
    public class AgentIdentity
    {
        public string AgentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        //  For Multi-tenancy or environment isolation (e.g., "dev", "prod", HR Dept")
        public string TenantId { get; set; } = string.Empty;

        //  Navigation properties
        public List<AgentRoleAssignment> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}