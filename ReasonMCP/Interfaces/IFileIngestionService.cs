using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IFileIngestionService
    {
        Task<bool> IngestSingleEnrichedObjectAsync(
            KnowledgeRecord record,
            CancellationToken cancellationToken
        );
    }
}