namespace ReasonMCP.Enrichment.Interfaces
{
    public interface IRAGChunkingUtility
    {
        IAsyncEnumerable<string> CreateChunks(string text);
    }
}