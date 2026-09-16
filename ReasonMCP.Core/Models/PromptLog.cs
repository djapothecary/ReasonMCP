namespace ReasonMCP.Models
{
    public class PromptLog
    {
        public string AgentId { get; set; } = "reason";
        public string UserUd { get; set; } = "apoth";
        public DateTime Created { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}