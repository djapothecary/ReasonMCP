using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IKnowledgebaseRecordIngestionService
    {
        Task<bool> IngestEnrichedKnowledgeBaseRecordAsync(
            KnowledgebaseRecord record,
            CancellationToken cancellationToken = default
        );
    }
}