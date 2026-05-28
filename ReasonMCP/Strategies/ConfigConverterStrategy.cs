using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Records;

namespace ReasonMCP.Strategies
{
    public class ConfigConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICodeChunkingProcessor _configChunkingProcessor;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestionQueueUpdaterService _updaterService;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<ConfigConverterStrategy> _logger;

        public ConfigConverterStrategy(
            IServiceScopeFactory scopeFactory,
            ICodeChunkingProcessor configChunkingProcessor,
            IIngestionQueueService ingestionQueue,
            IIngestionQueueUpdaterService updaterService,
            IOptions<CodebaseScanSettings> options,
            ILogger<ConfigConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _configChunkingProcessor = configChunkingProcessor;
            _ingestionQueue = ingestionQueue;
            _updaterService = updaterService;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.ConfigExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> ConvertForIngestionAsync(string filePath)
        {
            var scope = _scopeFactory.CreateScope();
            //  create Cancellation Token
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            IEnumerable<CodeChunk> chunks = [];
            chunks = await _configChunkingProcessor.ChunkFileAsync(filePath, cancellationToken);

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