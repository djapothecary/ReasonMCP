using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models.Tests
{
    public class Evidence
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("record")]
        public string Record { get; set; } = string.Empty;
    }
}