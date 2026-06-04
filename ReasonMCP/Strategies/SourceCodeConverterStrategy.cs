using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Processors;
using ReasonMCP.Records;

namespace ReasonMCP.Strategies
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
            return _settings.SourceCodeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> ConvertForIngestionAsync(
            string filePath)
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

            // Now send chunks off to Embedding
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;
            var codebaseUpsertOrchestratior = scope.ServiceProvider.GetRequiredService<CodebaseRecordUpsertOrchestrator>();
            return await codebaseUpsertOrchestratior.CodebaseChunkUpsertAsync(chunks, cancellationToken);
        }
    }
}