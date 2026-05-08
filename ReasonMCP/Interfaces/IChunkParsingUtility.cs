using ReasonMCP.Models;

namespace ReaconMCP.Interfaces
{
    public interface IChunkParsingUtility
    {
        Task<List<KnowledgeRecord>> ParseEnrichedMarkdownAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );
    }
}