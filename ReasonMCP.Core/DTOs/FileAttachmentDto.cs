using System.Text.Json.Serialization;

namespace ReasonMCP.Core.DTOs
{
    public class FileAttachmentDto
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}