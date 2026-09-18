using System.Text.Json.Serialization;
using ReasonMCP.Core.DTOs;

namespace ReasonMCP.Core.DTOS
{
    public class McpResourceTemplateListResponse
    {
        [JsonPropertyName("templates")]
        public List<McpResourceTemplate> Templates { get; set; } = new();
    }
}