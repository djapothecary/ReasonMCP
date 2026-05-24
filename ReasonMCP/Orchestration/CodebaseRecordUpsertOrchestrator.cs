using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Records;

namespace ReasonMCP.Orchestration
{
    public class CodebaseRecordUpsertOrchestrator
    {
        private readonly ICodebaseRecordIngestionService _ingestService;
        private ICodeChunkingProcessor __codeChunkingProcessor;
        private IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private ILogger<CodebaseRecordUpsertOrchestrator> _logger;

        public CodebaseRecordUpsertOrchestrator(
            ICodebaseRecordIngestionService ingestService,
            ICodeChunkingProcessor codeChunkingProcessor,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            ILogger<CodebaseRecordUpsertOrchestrator> logger
        )
        {
            _ingestService = ingestService;
            __codeChunkingProcessor = codeChunkingProcessor;
            _embeddingGenerator = embeddingGenerator;
            _logger = logger;
        }

        public async Task<bool> CodebaseChunkUpsertAsync(
            IEnumerable<CodeChunk> chunks,
            CancellationToken cancellationToken = default
        )
        {
            bool upsertSuccess = false;
            try
            {
                foreach (var chunk in chunks)
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
                    var record = new CodebaseEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = chunk.Content,
                        Vector = generatedEmbeddings.First().Vector,
                        FilePath = chunk.FilePath,
                        NodeType = chunk.NodeType,
                        NodeUri = chunk.NodeUri
                    };

                    upsertSuccess = await _ingestService.IngestEnrichedCodebaseRecordAsync(record, cancellationToken);
                    _logger.LogTrace($"Successfully upserted {record.NodeUri}", chunk.NodeUri);

                    Console.WriteLine($"Successfully upserted {chunk.NodeUri}");


                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return upsertSuccess;
        }
    }
}