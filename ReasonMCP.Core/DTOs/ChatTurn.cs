using System.Text.Json.Serialization;

namespace ReasonMCP.Core.DTOs
{
    public class ChatTurn
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}