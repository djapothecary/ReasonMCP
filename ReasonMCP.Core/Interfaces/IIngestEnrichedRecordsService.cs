using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IIngestEnrichedRecordsService
    {
        Task<bool> IngestEnrichedCodebaseRecordAsync(
            CodebaseVectorModel record,
            CancellationToken cancellationToken = default
        );

        Task<bool> IngestEnrichedDocumentRecordAsync(
            DocumentVectorModel record,
            CancellationToken cancellationToken = default
        );

        Task<bool> IngestEnrichedReferenceRecordAsync(
            ReferenceVectorModel record,
            CancellationToken cancellationToken = default
        );
    }
}