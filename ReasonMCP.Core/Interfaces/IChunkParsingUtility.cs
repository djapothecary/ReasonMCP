using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IChunkParsingUtility
    {
        Task<List<CodebaseVectorModel>> ParseEnrichedCodebaseMarkdownAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        Task<List<DocumentVectorModel>> ParseEnrichedDocumentMarkdownAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        Task<List<ReferenceVectorModel>> ParseEnrichedReferenceMarkdownAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );
    }
}