using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models.Skills
{
    public class SkillsMcpServer
    {
        [JsonPropertyName("{type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("args")]
        public List<string> Args { get; set; } = [];

        [JsonPropertyName("tools")]
        public List<string> Tools { get; set; } = [];
    }
}