namespace ReasonMCP.Models
{
    public class RagObject
    {
        public string Content { get; set; } = string.Empty;
        public string SourceHeader { get; set; } = "General";
        public int ChunkIndex { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}