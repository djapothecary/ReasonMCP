using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IDocumentIngestService
    {
        Task<bool> IngestEnrichedDocumentAsync(
            DocumentVectorModel record,
            CancellationToken cancellationToken = default
        );
    }
}