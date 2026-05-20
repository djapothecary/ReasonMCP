using ReasonMCP.DTOs;
namespace ReasonMCP.DTOs
{
    public class ChatRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public List<ChatMessage> History { get; set; } = new();
    }
}