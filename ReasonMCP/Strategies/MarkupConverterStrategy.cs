using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Orchestration;
using ReasonMCP.Processors;
using ReasonMCP.Records;

namespace ReasonMCP.Strategies
{
    public class MarkupConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICodeChunkingProcessor _markupChunkingProcessor;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestionQueueUpdaterService _updaterService;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<MarkupConverterStrategy> _logger;

        public MarkupConverterStrategy(
            IServiceScopeFactory scopeFactory,
            ICodeChunkingProcessor markupChunkingProcessor,
            IIngestionQueueService ingestionQueue,
            IIngestionQueueUpdaterService updaterService,
            IOptions<CodebaseScanSettings> options,
            ILogger<MarkupConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _markupChunkingProcessor = markupChunkingProcessor;
            _ingestionQueue = ingestionQueue;
            _updaterService = updaterService;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.MarkupExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> ConvertForIngestionAsync(string filePath)
        {
            var scope = _scopeFactory.CreateScope();
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            IEnumerable<CodeChunk> chunks = [];
            chunks = await _markupChunkingProcessor.ChunkFileAsync(filePath, cancellationToken);

            bool chunkUpsertSuccess = false;
            try
            {
                var codebaseUpsertOrchestratior = scope.ServiceProvider.GetRequiredService<CodebaseRecordUpsertOrchestrator>();
                chunkUpsertSuccess = await codebaseUpsertOrchestratior.CodebaseChunkUpsertAsync(chunks, cancellationToken);

                await _updaterService.MarkConversionStatus(filePath, chunkUpsertSuccess, cancellationToken);
            }
            catch (Exception conversionUpsertEx)
            {
                await _ingestionQueue.MarkFailedExceptionAsync(filePath, conversionUpsertEx.Message, cancellationToken);
            }

            return chunkUpsertSuccess;
        }
    }
}