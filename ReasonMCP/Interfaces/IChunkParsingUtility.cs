using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IChunkParsingUtility
    {
        Task<List<KnowledgebaseRecord>> ParseEnrichedMarkdownAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );
    }
}