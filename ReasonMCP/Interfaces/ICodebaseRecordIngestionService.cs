using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface ICodebaseRecordIngestionService
    {
        Task<bool> IngestEnrichedCodebaseRecordAsync(
            CodebaseEntity record,
            CancellationToken cancellationToken = default
        );
    }
}