using ReasonMCP.Core.Models;

namespace ReasonMCP.Enrichment.Interfaces
{
    public interface ICodebaseProcessor
    {
        Task<FileIngestionRecord> GetNextCodebaseFileAsync(
            CancellationToken cancellationToken
        );
    }
}