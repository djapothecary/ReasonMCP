using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models
{
    public class LspServerConfig
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("args")]
        public List<string> Args { get; set; } = [];

        [JsonPropertyName("cwd")]
        public string Cwd { get; set; } = string.Empty;

        [JsonPropertyName("fileExtensions")]
        public Dictionary<string, string> FileExtensions { get; set; } = new();

        [JsonPropertyName("warmupTimeouts")]
        public int WarmupTimeouts { get; set; }
    }
}