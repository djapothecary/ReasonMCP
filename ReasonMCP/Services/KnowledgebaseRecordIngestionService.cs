using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Services
{
    public class KnowledgebaseRecordIngestionService : IKnowledgebaseRecordIngestionService
    {
        private readonly VectorStoreCollection<string, KnowledgebaseRecord> _collection;
        private readonly ILogger<KnowledgebaseRecordIngestionService> _logger;

        public KnowledgebaseRecordIngestionService(
            VectorStore vectorStore,
            ILogger<KnowledgebaseRecordIngestionService> logger
        )
        {
            _collection = vectorStore.GetCollection<string, KnowledgebaseRecord>("ReasonContext");
            _logger = logger;
        }

        public async Task<bool> IngestEnrichedKnowledgeBaseRecordAsync(
            KnowledgebaseRecord record,
            CancellationToken cancellationToken)
        {
            try
            {
                //  1.  Ensure the collection actually existss
                await _collection.EnsureCollectionExistsAsync(cancellationToken);

                //  2.  Upsert the record. If it fails, it throws an exception.
                await _collection.UpsertAsync(record, cancellationToken: cancellationToken);
                return true;
            }
            catch (VectorStoreException vEx)
            {
                // Log specifically that the Vector DB rejected the upsert
                _logger.LogError(vEx, "Vector database error during upsert for chunk {ChunkIndex} of {Source}", record.ChunkIndex, record?.Metadata?["source"]);
                return false;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest RagObject for source: {Source}", record?.Metadata?.GetValueOrDefault("source"));
                return false;
            }
        }
    }
}