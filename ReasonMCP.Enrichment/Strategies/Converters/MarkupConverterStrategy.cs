using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Processors;
using ReasonMCP.Core.Records;
using ReasonMCP.Core.Services.Enrichment;
using ReasonMCP.Enrichment.Interfaces;

namespace ReasonMCP.Enrichment.Strategies.Converters
{
    public class MarkupConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MarkupChunkingProcessor _markupChunkingProcessor;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<MarkupConverterStrategy> _logger;

        public MarkupConverterStrategy(
            IServiceScopeFactory scopeFactory,
            MarkupChunkingProcessor markupChunkingProcessor,
            IIngestionQueueService ingestionQueue,
            IOptionsMonitor<CodebaseScanSettings> options,
            ILogger<MarkupConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _markupChunkingProcessor = markupChunkingProcessor;
            _ingestionQueue = ingestionQueue;
            _settings = options.CurrentValue;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.MarkupExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> ConvertForIngestionAsync(
            string filePath,
            bool writeConvertedOutput,
            CancellationToken cancellationToken = default
        )
        {
            using var scope = _scopeFactory.CreateScope();

            IEnumerable<CodeChunk> chunks = [];
            chunks = await _markupChunkingProcessor.ChunkFileAsync(
                filePath,
                cancellationToken
            );

            bool chunkUpsertSuccess = false;
            try
            {
                var codebaseRecordIngestService = scope
                    .ServiceProvider
                    .GetRequiredService<ICodebaseRecordIngestionService>();

                chunkUpsertSuccess = await codebaseRecordIngestService
                    .CodebaseChunkUpsertAsync(
                        chunks,
                        cancellationToken
                    );

                await _ingestionQueue.MarkConversionCompleteAsync(
                    filePath,
                    cancellationToken
                );
            }
            catch (Exception conversionUpsertEx)
            {
                await _ingestionQueue.MarkFailedExceptionAsync(
                    filePath,
                    conversionUpsertEx.Message,
                    cancellationToken
                );
            }

            return chunkUpsertSuccess;
        }
    }
}