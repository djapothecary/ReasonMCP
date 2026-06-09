using System.Text.Json.Serialization;

namespace ReasonMCP.DTOs
{
    public class FileAttachmentDto
    {
        [JsonPropertyName("fileName")]
        public string Filename { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}