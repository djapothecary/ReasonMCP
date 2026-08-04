using ReasonMCP.Models;

namespace ReasonMCP.Interfaces.IEnrichment
{
    public interface ICodebaseProcessor
    {
        Task<FileIngestionRecord> GetNextCodebaseFileAsync(
            CancellationToken cancellationToken
        );
    }
}