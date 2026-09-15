namespace ReasonMCP.Models
{
    public class AgentRole
    {
        public string AgentId { get; set; } = string.Empty; //  bella, esper, reason, etc
        public string Name { get; set; } = string.Empty;
        public List<AgentPermission> Permissions { get; set; } = new();
    }
}