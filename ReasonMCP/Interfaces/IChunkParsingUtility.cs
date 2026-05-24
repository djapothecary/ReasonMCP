using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IChunkParsingUtility
    {
        Task<List<KnowledgebaseEntity>> ParseEnrichedMarkdownAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );
    }
}