using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Processors;
using ReasonMCP.Records;
using ReasonMCP.Services.Enrichment;

namespace ReasonMCP.Strategies.Converters
{
    public class SourceCodeConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CSharpRoslynChunkingProcessor _csharpCodeChunkingProcessor;
        private readonly TypeScriptChunkingProcessor _typeScriptChunkingProcessor;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<SourceCodeConverterStrategy> _logger;

        public SourceCodeConverterStrategy(
            IServiceScopeFactory scopeFactory,
            CSharpRoslynChunkingProcessor csharpCodeChunkingProcessor,
            TypeScriptChunkingProcessor typeScriptChunkingProcessor,
            IOptions<CodebaseScanSettings> options,
            ILogger<SourceCodeConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _csharpCodeChunkingProcessor = csharpCodeChunkingProcessor;
            _typeScriptChunkingProcessor = typeScriptChunkingProcessor;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.SourceCodeExtensions.Contains(
                fileExtension,
                StringComparer.OrdinalIgnoreCase
            );
        }

        public async Task<bool> ConvertForIngestionAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            IEnumerable<CodeChunk> chunks = [];
            var fileExtension = Path.GetExtension(filePath);
            var scope = _scopeFactory.CreateScope();

            if (_settings.CSharpExtensions.Contains(fileExtension))
            {
                chunks = await _csharpCodeChunkingProcessor.ChunkFileAsync(filePath);
            }
            else // if (_settings.TypeScriptExtensions.Contains(fileExtension))
            {
                chunks = await _typeScriptChunkingProcessor.ChunkFileAsync(filePath);
            }

            var codebaseUpsertOrchestratior = scope
                .ServiceProvider
                .GetRequiredService<CodebaseRecordIngestService>();

            return await codebaseUpsertOrchestratior
                .CodebaseChunkUpsertAsync(
                    chunks,
                    cancellationToken
                );
        }
    }
}