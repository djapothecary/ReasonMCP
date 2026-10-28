using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models
{
    public class LspConfigRoot
    {
        [JsonPropertyName("lspServers")]
        public Dictionary<string, LspServerConfig> LspServers { get; set; } = new();
    }
}