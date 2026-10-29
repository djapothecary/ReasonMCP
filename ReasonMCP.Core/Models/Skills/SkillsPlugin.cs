using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models.Skills
{
    public class SkillsPlugin
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; } = [];

        [JsonPropertyName("agents")]
        public List<string> Agents { get; set; } = [];

        [JsonPropertyName("mcpServers")]
        public SkillsBinLog McpServers { get; set; } = new();
    }
}