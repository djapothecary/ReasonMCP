using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Orchestration
{
    public class CodebaseRecordUpsertOrchestrator
    {
        private readonly ICodebaseRecordIngestionService _ingestService;
        private ICodeChunkingStrategy __codeChunkingService;
        private IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private ILogger<CodebaseRecordUpsertOrchestrator> _logger;

        public CodebaseRecordUpsertOrchestrator(
            ICodebaseRecordIngestionService ingestService,
            ICodeChunkingStrategy codeChunkingService,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            ILogger<CodebaseRecordUpsertOrchestrator> logger
        )
        {
            _ingestService = ingestService;
            __codeChunkingService = codeChunkingService;
            _embeddingGenerator = embeddingGenerator;
            _logger = logger;
        }

        //  left off here.
        //  need to add call to AST chunking (verified, just call the ast)
        //  then embedding generator (verified, just call the embeddingGenerator)
        //  then upsert
        //  add try/catch to this and KnowledgebaseRecordOrchestrator
        //  to update failed records
    }
}