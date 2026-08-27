using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Processors;
using ReasonMCP.Records;

namespace ReasonMCP.Strategies.Converters
{
    public class ConfigConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConfigChunkingProcessor _configChunkingProcessor;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<ConfigConverterStrategy> _logger;

        public ConfigConverterStrategy(
            IServiceScopeFactory scopeFactory,
            ConfigChunkingProcessor configChunkingProcessor,
            IIngestionQueueService ingestionQueue,
            IOptionsMonitor<CodebaseScanSettings> options,
            ILogger<ConfigConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _configChunkingProcessor = configChunkingProcessor;
            _ingestionQueue = ingestionQueue;
            _settings = options.CurrentValue;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.ConfigExtensions.Contains(
                fileExtension,
                StringComparer.OrdinalIgnoreCase
            );
        }

        public async Task<bool> ConvertForIngestionAsync(
            string filePath,
            bool writeConvertedOutput,
            CancellationToken cancellationToken
        )
        {
            using var scope = _scopeFactory.CreateScope();

            IEnumerable<CodeChunk> chunks = [];
            chunks = await _configChunkingProcessor.ChunkFileAsync(
                filePath,
                cancellationToken
            );

            bool chunkUpsertSuccess = false;
            try
            {
                var codebaseUpsertOrchestratior = scope
                    .ServiceProvider
                    .GetRequiredService<ICodebaseRecordIngestionService>();
                chunkUpsertSuccess = await codebaseUpsertOrchestratior
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