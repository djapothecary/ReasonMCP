using Microsoft.AspNetCore.Builder;
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
    /// <summary>
    /// This classintentionally uses Boilerplate constructors
    /// </summary>
    public class SqlScriptConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ICodeChunkingProcessor _sqlScriptChunkProcessor;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<SqlScriptConverterStrategy> _logger;

        private static readonly string[] _supportedExtensions =
        [
            ".sql"
        ];

        public SqlScriptConverterStrategy(
            IServiceScopeFactory scopeFactory,
            ICodeChunkingProcessor sqlScriptChunkProcessor,
            IOptions<CodebaseScanSettings> options,
            ILogger<SqlScriptConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _sqlScriptChunkProcessor = sqlScriptChunkProcessor;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _supportedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> ConvertForIngestionAsync(string filePath)
        {
            var scope = _scopeFactory.CreateScope();
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            IEnumerable<CodeChunk> chunks = [];
            chunks = await _sqlScriptChunkProcessor.ChunkFileAsync(filePath, cancellationToken);

            var codebaseUpsertOrchestratior = scope.ServiceProvider.GetRequiredService<CodebaseRecordUpsertOrchestrator>();
            return await codebaseUpsertOrchestratior.CodebaseChunkUpsertAsync(chunks, cancellationToken);
        }
    }
}