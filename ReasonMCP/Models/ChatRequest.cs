using System.Text.Json.Serialization;

namespace ReasonMCP.Models
{
    public class ChatRequest
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public ChatResponse? ChatResponse { get; set; }

        [JsonPropertyName("message")]
        public List<ChatMessage> Messages { get; set; } = [];
        public List<ChatMessage> ChatHistory { get; set; } = [];
    }
}