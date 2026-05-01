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
            RagObject ragObj,
            IEmbeddingGenerator<string, Embedding<float>> embeddgingGenerator,
            CancellationToken cancellationToken)
        {
            try
            {
                //  1.  Ensure the collection actually existss
                await _collection.EnsureCollectionExistsAsync(cancellationToken);

                //  2.  Generate embeddings
                var embeddings = await embeddgingGenerator.GenerateAsync(
                    new[] { ragObj.Content },
                    cancellationToken: cancellationToken
                );

                //  3.  Map to knowledge record
                var record = new KnowledgeRecord
                {
                    Text = ragObj.Content,
                    Vector = embeddings.First().Vector.ToArray(),
                    Source = ragObj.Metadata["source"].ToString(),
                    HeaderContext = ragObj.SourceHeader,
                    ChunkIndex = ragObj.ChunkIndex
                };

                //  4.  Upsert the record. If it fails, it throws an exception.
                await _collection.UpsertAsync(record, cancellationToken: cancellationToken);
                return true;
            }
            catch (VectorStoreException vEx)
            {
                // Log specifically that the Vector DB rejected the upsert
                _logger.LogError(vEx, "Vector database error during upsert for chunk {ChunkIndex} of {Source}", ragObj.ChunkIndex, ragObj.Metadata["source"]);
                return false;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest RagObject for source: {Source}", ragObj.Metadata.GetValueOrDefault("source"));
                return false;
            }
        }
    }
}