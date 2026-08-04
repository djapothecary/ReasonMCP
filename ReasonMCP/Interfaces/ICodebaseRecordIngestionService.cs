using ReasonMCP.Models;
using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface ICodebaseRecordIngestionService
    {
        Task<bool> CodebaseChunkUpsertAsync(
            IEnumerable<CodeChunk> chunks,
            CancellationToken cancellationToken = default
        );
    }
}