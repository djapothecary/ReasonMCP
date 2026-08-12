using ReasonMCP.Models;

namespace ReasonMCP.Interfaces.IEnrichment
{
    public interface IDocumentsProcessor
    {
        Task<FileIngestionRecord> GetNextDocumentFileAsync(
            CancellationToken cancellationToken
        );

        Task<bool> IngestDocumentRecordAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        Task MoveMarkdownsToProcessedAsync();

        Task<string> ConvertToMarkdownPathAsync(
            string filePath
        );
    }
}