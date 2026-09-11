using System.Text.Json.Serialization;
using ReasonMCP.DTOs;

namespace ReasonMCP.DTOS
{
    public class McpResourceTemplateListResponse
    {
        [JsonPropertyName("templates")]
        public List<McpResourceTemplate> Templates { get; set; } = new();
    }
}