using System.Text.Json.Serialization;
using Evidence = ReasonMCP.Core.Models;

namespace ReasonMCP.Core.Models.Tests
{
    public class Finding
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("classification")]
        public string Classification { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public string Confidence { get; set; } = string.Empty;

        [JsonPropertyName("evidence")]
        public List<Evidence> Evidence { get; set; } = [];

        [JsonPropertyName("next_step")]
        public string NextStep { get; set; } = string.Empty;
    }
}