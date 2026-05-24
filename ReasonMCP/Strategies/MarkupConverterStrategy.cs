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
        private readonly MarkupChunkingProcessor _markupChunkingProcessor;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<MarkupConverterStrategy> _logger;

        public MarkupConverterStrategy(
            IServiceScopeFactory scopeFactory,
            MarkupChunkingProcessor markupChunkingProcessor,
            IOptions<CodebaseScanSettings> options,
            ILogger<MarkupConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _markupChunkingProcessor = markupChunkingProcessor;
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

            var codebaseUpsertOrchestratior = scope.ServiceProvider.GetRequiredService<CodebaseRecordUpsertOrchestrator>();
            return await codebaseUpsertOrchestratior.CodebaseChunkUpsertAsync(chunks, cancellationToken);
        }
    }
}