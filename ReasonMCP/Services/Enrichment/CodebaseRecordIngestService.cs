using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Records;

namespace ReasonMCP.Services.Enrichment
{
    public class CodebaseRecordIngestService : ICodebaseRecordIngestionService
    {
        private readonly VectorStoreCollection<string, CodebaseVectorModel> _collection;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestEnrichedRecordsService _ingestionService;
        private IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private ILogger<CodebaseRecordIngestService> _logger;

        //  8000 chars is roughly 2000 tokens.  Extremely safe for Nomic and local LLMs
        private const int MaxCharsPerChunk = 8000;

        public CodebaseRecordIngestService(
            VectorStore vectorStore,
            IIngestionQueueService ingestionQueue,
            IIngestEnrichedRecordsService ingestionService,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            ILogger<CodebaseRecordIngestService> logger
        )
        {
            _collection = vectorStore.GetCollection<string, CodebaseVectorModel>("Codebase");
            _ingestionQueue = ingestionQueue;
            _ingestionService = ingestionService;
            _embeddingGenerator = embeddingGenerator;
            _logger = logger;
        }

        public async Task<bool> CodebaseChunkUpsertAsync(
            IEnumerable<CodeChunk> chunks,
            CancellationToken cancellationToken = default
        )
        {
            // placeHolder for chunk.FilenPath for error handlings
            var chunkFilePath = string.Empty;
            bool upsertSuccess = false;
            try
            {
                //  Flattens any sub-chunks directly into the main iteration
                var safeChunks = chunks.SelectMany(EnforceTokenLimit);
                foreach (var chunk in safeChunks)
                {
                    if (string.IsNullOrWhiteSpace(chunk.Content))
                        continue;

                    _logger.LogTrace("Generating embeddings for Code node: {NodeUri}", chunk.NodeUri);

                    //  1.  Generate the Embedding
                    var generatedEmbeddings = await _embeddingGenerator.GenerateAsync(
                        new[] { chunk.Content },
                        cancellationToken: cancellationToken
                    );

                    //  2.  Map the DTO to the Vector Database Entity
                    var record = new CodebaseVectorModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = chunk.Content,
                        Vector = generatedEmbeddings.First().Vector,
                        FilePath = chunk.FilePath,
                        NodeType = chunk.NodeType,
                        NodeUri = chunk.NodeUri
                    };

                    chunkFilePath = chunk.FilePath;

                    upsertSuccess = await _ingestionService.IngestEnrichedCodebaseRecordAsync(
                        record,
                        cancellationToken
                    );

                    _logger.LogTrace($"Successfully upserted {record.NodeUri}", chunk.NodeUri);

                    int chunkCount = 1;
                    Console.WriteLine($"Successfully upserted {chunkFilePath}  Chunk Count: {chunkCount}", chunkFilePath, chunkCount);
                    chunkCount++;
                }
            }
            catch (Exception ex)
            {
                await _ingestionQueue.MarkFailedExceptionAsync(chunkFilePath, "An error occured during chunk upsert: " + ex.Message, cancellationToken);
                return false;
            }

            return upsertSuccess;
        }

        /// <summary>
        /// Evaluates a chunk's length and streams sub-chunks if it exceeds token safety-limit.
        /// </summary>
        private IEnumerable<CodeChunk> EnforceTokenLimit(CodeChunk originalChunk)
        {
            if (string.IsNullOrWhiteSpace(originalChunk.Content) ||
                originalChunk.Content.Length <= MaxCharsPerChunk)
            {
                //  yield the original untouched if it's safe
                yield return originalChunk;
                yield break;
            }

            //  It's too big.  Slice it by line to avoid cutting words/syntax exactly in half
            var lines = originalChunk.Content.Split('\n');
            var currentContent = new StringBuilder();
            int partNumber = 1;

            foreach (var line in lines)
            {
                if (currentContent.Length + line.Length > MaxCharsPerChunk && currentContent.Length > 0)
                {
                    yield return CreateFragmentedChunk(originalChunk, currentContent.ToString(), partNumber);

                    currentContent.Clear();
                    partNumber++;
                }

                currentContent.AppendLine(line);
            }

            //  flush the remainder
            if (currentContent.Length > 0)
            {
                yield return CreateFragmentedChunk(originalChunk, currentContent.ToString(), partNumber);
            }
        }

        /// <summary>
        /// Creates a clone of the original chunk with updated content and a "_Partx" appended to the NodeUri.
        /// </summary>
        private static CodeChunk CreateFragmentedChunk(
            CodeChunk original,
            string newContent,
            int partNumber
        )
        {
            return new CodeChunk(
                Content: newContent.TrimEnd(),
                FilePath: original.FilePath,
                NodeUri: $"{original.NodeUri}_Part{partNumber}", // Tells Reason it's a fragment!
                NodeType: original.NodeType,
                StartLine: original.StartLine,
                EndLine: original.EndLine,
                Metadata: original.Metadata
            );
        }
    }
}