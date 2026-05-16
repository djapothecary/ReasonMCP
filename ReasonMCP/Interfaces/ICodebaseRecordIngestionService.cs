using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface ICodebaseRecordIngestionService
    {
        Task<bool> IngestEnrichedCodebaseRecordAsync(
            CodebaseRecord record,
            CancellationToken cancellationToken = default
        );
    }
}