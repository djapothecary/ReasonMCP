using ReasonMCP.Models;

namespace ReasonMCP.Interfaces.IEnrichment
{
    public interface IReferenceDataProcessor
    {
        Task<FileIngestionRecord> GetNextReferenceFileAsync(
            CancellationToken cancellationToken
        );

        Task<bool> IngestReferenceFileRecordAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        Task GetFileForUpsertAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        Task MoveMarkdownsToProcessedAsync();

        Task<string> ConvertToMarkdownPathAsync(
            string filePath
        );
    }
}