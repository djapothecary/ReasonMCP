using ReasonMCP.DTOs;
namespace ReasonMCP.DTOs
{
    public class ReasonChatRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public List<ReasonChatMessage> History { get; set; } = new();
    }
}