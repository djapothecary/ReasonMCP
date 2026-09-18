using ReasonMCP.Core.Models;

namespace ReasonMCP.Enrichment.Interfaces
{
    public interface IDocumentIngestService
    {
        Task<bool> IngestEnrichedDocumentAsync(
            DocumentVectorModel record,
            CancellationToken cancellationToken = default
        );
    }
}