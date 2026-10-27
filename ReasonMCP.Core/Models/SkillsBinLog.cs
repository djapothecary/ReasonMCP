using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models
{
    public class SkillsBinLog
    {
        [JsonPropertyName("binlog")]
        public SkillsMcpServer BinLog { get; set; } = new();
    }
}