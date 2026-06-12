using System.Text.Json.Serialization;
using ReasonMCP.DTOs;

namespace ReasonMCP.DTOs
{
    // A temporary DTO to catch whatever VS Code is throwing at us
    public class VSCodeChatPayloadDto
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("agentId")]
        public string AgentId { get; set; } = "reason"; //  Default to reason for safety

        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("history")]
        public List<ChatTurn> History { get; set; } = [];

        [JsonPropertyName("attachments")]
        public List<FileAttachmentDto> Attachments { get; set; } = [];
    }

}