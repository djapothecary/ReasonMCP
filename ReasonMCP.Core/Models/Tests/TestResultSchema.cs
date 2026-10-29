using System.Text.Json.Serialization;
using Finding = ReasonMCP.Core.Models;

namespace ReasonMCP.Core.Models.Tests
{
    public class TestResultSchema
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("proposed_only")]
        public bool ProposedOnly { get; set; }

        [JsonPropertyName("findings")]
        public List<Finding> Findings { get; set; } = [];

        [JsonPropertyName("limitations")]
        public List<object> Limitations { get; set; } = [];

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}