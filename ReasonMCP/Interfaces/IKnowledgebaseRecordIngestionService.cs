using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IKnowledgebaseRecordIngestionService
    {
        Task<bool> IngestEnrichedKnowledgeBaseRecordAsync(
            KnowledgebaseEntity record,
            CancellationToken cancellationToken = default
        );
    }
}