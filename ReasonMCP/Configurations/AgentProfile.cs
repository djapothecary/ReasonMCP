using ReasonMCP.Configurations;

namespace ReasonMCP.Configurations
{
    public class AgentProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
        public AgentExecutionSettings ExecutionSettings { get; set; } = new();
        public AgentPermissions Permissions { get; set; } = new();
    }
}