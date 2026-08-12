using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Services.Enrichment
{
    public class IngestEnrichedRecordsService : IIngestEnrichedRecordsService
    {
        private readonly VectorStoreCollection<string, CodebaseVectorModel> _codebaseCollection;
        private readonly VectorStoreCollection<string, DocumentVectorModel> _documentCollection;
        private readonly VectorStoreCollection<string, ReferenceVectorModel> _referenfceCollection;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly ILogger<IngestEnrichedRecordsService> _logger;

        public IngestEnrichedRecordsService(
            VectorStore codebaseVectorStore,
            VectorStore documentVectorStore,
            VectorStore referenceVectorStore,
            IIngestionQueueService ingestionQueue,
            ILogger<IngestEnrichedRecordsService> logger
        )
        {
            _codebaseCollection = codebaseVectorStore.GetCollection<string, CodebaseVectorModel>("Codebase");
            _documentCollection = documentVectorStore.GetCollection<string, DocumentVectorModel>("Documents");
            _referenfceCollection = referenceVectorStore.GetCollection<string, ReferenceVectorModel>("Reference");
            _ingestionQueue = ingestionQueue;
            _logger = logger;
        }

        public async Task<bool> IngestEnrichedCodebaseRecordAsync(
            CodebaseVectorModel record,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                //  1.  Ensure the collection actually existss
                await _codebaseCollection.EnsureCollectionExistsAsync(cancellationToken);

                //  2.  Upsert the record. If it fails, it throws an exception.
                await _codebaseCollection.UpsertAsync(
                    record,
                    cancellationToken: cancellationToken
                );

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

        public async Task<bool> IngestEnrichedDocumentRecordAsync(
            DocumentVectorModel record,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                //  1.  Ensure the collection actually existss
                await _documentCollection.EnsureCollectionExistsAsync(cancellationToken);

                //  2.  Upsert the record. If it fails, it throws an exception.
                await _documentCollection.UpsertAsync(record, cancellationToken: cancellationToken);
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

        public async Task<bool> IngestEnrichedReferenceRecordAsync(
            ReferenceVectorModel record,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                //  1.  Ensure the collection actually existss
                await _referenfceCollection.EnsureCollectionExistsAsync(cancellationToken);

                //  2.  Upsert the record. If it fails, it throws an exception.
                await _referenfceCollection.UpsertAsync(record, cancellationToken: cancellationToken);
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