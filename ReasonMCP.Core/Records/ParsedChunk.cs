namespace ReasonMCP.Records
{
    public record ParsedChunk(
        string Text,
        string Topic,
        string Source,
        int ChunkIndex,
        Dictionary<string, string> Metadata
    );
}