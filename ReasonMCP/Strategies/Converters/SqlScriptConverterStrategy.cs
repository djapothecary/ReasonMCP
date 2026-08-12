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
    /// <summary>
    /// This classintentionally uses Boilerplate constructors
    /// </summary>
    public class SqlScriptConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private SqlScriptChunkingProcessor _sqlScriptChunkProcessor;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<SqlScriptConverterStrategy> _logger;

        private static readonly string[] _supportedExtensions =
        [
            ".sql"
        ];

        public SqlScriptConverterStrategy(
            IServiceScopeFactory scopeFactory,
            SqlScriptChunkingProcessor sqlScriptChunkProcessor,
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

        public async Task<bool> ConvertForIngestionAsync(
            string filePath,
            bool writeConvertedOutput,
            CancellationToken cancellationToken = default
        )
        {
            var scope = _scopeFactory.CreateScope();

            IEnumerable<CodeChunk> chunks = [];
            chunks = await _sqlScriptChunkProcessor.ChunkFileAsync(
                filePath,
                cancellationToken
            );

            var codebaseRecordIngestService = scope
                .ServiceProvider
                .GetRequiredService<CodebaseRecordIngestService>();

            return await codebaseRecordIngestService
                .CodebaseChunkUpsertAsync(
                    chunks,
                    cancellationToken
            );
        }
    }
}