using System.Text.Json.Serialization;

namespace ReasonMCP.Core.DTOs
{
    public class McpResourceTemplate
    {
        [JsonPropertyName("uriTemplate")]
        public string UriTemplate { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;
    }
}