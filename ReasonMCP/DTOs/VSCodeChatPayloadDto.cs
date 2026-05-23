using System.Text.Json.Serialization;
using ReasonMCP.DTOs;

namespace ReasonMCP.DTOs
{
    // A temporary DTO to catch whatever VS Code is throwing at us
    public class VSCodeChatPayloadDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("history")]
        public List<ChatTurn> History { get; set; } = [];

    }

}