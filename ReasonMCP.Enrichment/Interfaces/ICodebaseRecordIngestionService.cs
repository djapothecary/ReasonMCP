using ReasonMCP.Core.Models;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Enrichment.Interfaces
{
    public interface ICodebaseRecordIngestionService
    {
        Task<bool> CodebaseChunkUpsertAsync(
            IEnumerable<CodeChunk> chunks,
            CancellationToken cancellationToken = default
        );
    }
}