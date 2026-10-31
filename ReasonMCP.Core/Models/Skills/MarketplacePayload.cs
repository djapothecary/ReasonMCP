using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models.Skills
{
    public class MarketplacePayload
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("owner")]
        public Dictionary<string, string> Owner { get; set; } = new();

        [JsonPropertyName("plugins")]
        public List<MarketplacePlugin> Plugins { get; set; } = [];
    }
}