namespace ReasonMCP.Interfaces
{
    public interface IRAGChunkingUtility
    {
        IAsyncEnumerable<string> CreateChunks(string text);
    }
}