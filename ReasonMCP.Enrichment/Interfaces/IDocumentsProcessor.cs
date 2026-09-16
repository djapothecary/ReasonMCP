using ReasonMCP.Core.Models;

namespace ReasonMCP.Enrichment.Interfaces
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