using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Services
{
    public class FileIngestionService : IFileIngestionService
    {
        private readonly VectorStoreCollection<string, KnowledgeRecord> _collection;
        private readonly ILogger<FileIngestionService> _logger;

        public FileIngestionService(
            VectorStore vectorStore,
            ILogger<FileIngestionService> logger
        )
        {
            _collection = vectorStore.GetCollection<string, KnowledgeRecord>("ReasonContext");
            _logger = logger;
        }

        public async Task<bool> IngestSingleEnrichedObjectAsync(
            KnowledgeRecord record,
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