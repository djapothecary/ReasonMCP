using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models.Skills
{
    public class MarketplacePlugin
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}