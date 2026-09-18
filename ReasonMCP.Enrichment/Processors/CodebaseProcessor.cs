using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Models;
using ReasonMCP.Enrichment.Interfaces;

namespace ReasonMCP.Enrichment.Processors
{
    public class CodebaseProcessor : ICodebaseProcessor
    {
        private readonly IChunkParsingUtility _chunkParser;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestEnrichedRecordsService _ingestService;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly IOptionsMonitor<StorageConfigSettings> _options;
        private readonly ILogger<CodebaseProcessor> _logger;

        public CodebaseProcessor(
            IChunkParsingUtility chunkParser,
            IIngestionQueueService ingestionQueue,
            IIngestEnrichedRecordsService ingestService,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IOptionsMonitor<StorageConfigSettings> options,
            ILogger<CodebaseProcessor> logger
        )
        {
            _chunkParser = chunkParser;
            _ingestionQueue = ingestionQueue;
            _ingestService = ingestService;
            _embeddingGenerator = embeddingGenerator;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Dequeue the next Codebase record from
        /// IngestionQueue
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileIngestionRecord> GetNextCodebaseFileAsync(
            CancellationToken cancellationToken
        )
        {
            var file = await _ingestionQueue.DequeueNextFileAsync(
                "Codebase",
                cancellationToken
            );

            await Task.Delay(500, cancellationToken);
            return file!;
        }
    }
}