using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Services
{
    public class CodebaseRecordIngestionService : ICodebaseRecordIngestionService
    {
        private readonly VectorStoreCollection<string, CodebaseEntity> _collection;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly ILogger<CodebaseRecordIngestionService> _logger;

        public CodebaseRecordIngestionService(
            VectorStore vectorStore,
            IIngestionQueueService ingestionQueue,
            ILogger<CodebaseRecordIngestionService> logger
        )
        {
            _collection = vectorStore.GetCollection<string, CodebaseEntity>("ReasonContext");
            _ingestionQueue = ingestionQueue;
            _logger = logger;
        }

        public async Task<bool> IngestEnrichedCodebaseRecordAsync(
            CodebaseEntity record,
            CancellationToken cancellationToken = default
        )
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
                _logger.LogError(vEx, "Vector database error during upsert for File {FilePath}", record?.Metadata?["source"]);
                await _ingestionQueue.MarkFailedExceptionAsync(record!.FilePath!, "Upsert failed: " + vEx.Message, cancellationToken);
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