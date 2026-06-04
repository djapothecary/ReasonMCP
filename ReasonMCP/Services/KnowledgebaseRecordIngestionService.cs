using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Services
{
    public class KnowledgebaseRecordIngestionService : IKnowledgebaseRecordIngestionService
    {
        private readonly VectorStoreCollection<string, KnowledgebaseEntity> _collection;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly ILogger<KnowledgebaseRecordIngestionService> _logger;

        public KnowledgebaseRecordIngestionService(
            VectorStore vectorStore,
            IIngestionQueueService ingestionQueue,
            ILogger<KnowledgebaseRecordIngestionService> logger
        )
        {
            _collection = vectorStore.GetCollection<string, KnowledgebaseEntity>("DocumentsContext");
            _ingestionQueue = ingestionQueue;
            _logger = logger;
        }

        public async Task<bool> IngestEnrichedKnowledgeBaseRecordAsync(
            KnowledgebaseEntity record,
            CancellationToken cancellationToken)
        {
            try
            {
                //  1.  Ensure the collection actually existss
                await _collection.EnsureCollectionExistsAsync(cancellationToken);

                //  2.  Upsert the record. If it fails, it throws an exception.
                await _collection.UpsertAsync(record, cancellationToken: cancellationToken);

                //  3.  Update Ingestion Queue
                await _ingestionQueue.MarkCompleteAsync(record.Source!, cancellationToken);
                return true;
            }
            catch (VectorStoreException vEx)
            {
                // Log specifically that the Vector DB rejected the upsert
                _logger.LogError(vEx, "Vector database error during upsert for chunk {ChunkIndex} of {Source}", record.ChunkIndex, record?.Metadata?["source"]);
                await _ingestionQueue.MarkIngestionFailedAsync(record!.Source!, "Vector database error during upsert: " + vEx.Message, cancellationToken);
                return false;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest RagObject for source: {Source}", record?.Metadata?.GetValueOrDefault("source"));
                await _ingestionQueue.MarkIngestionFailedAsync(record!.Source!, "Failed to ingest RagObject for source: " + ex.Message, cancellationToken);
                return false;
            }
        }
    }
}