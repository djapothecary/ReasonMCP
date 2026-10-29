using System.Text.Json.Serialization;

namespace ReasonMCP.Core.Models.Skills
{
    public class SkillsBinLog
    {
        [JsonPropertyName("binlog")]
        public SkillsMcpServer BinLog { get; set; } = new();
    }
}