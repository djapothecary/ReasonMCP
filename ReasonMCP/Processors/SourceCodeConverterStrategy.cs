using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configuration;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    public class SourceCodeConverterStrategy : IFileConverterStrategy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<SourceCodeConverterStrategy> _logger;

        public SourceCodeConverterStrategy(
            IServiceScopeFactory scopeFactory,
            IOptions<CodebaseScanSettings> options,
            ILogger<SourceCodeConverterStrategy> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.SourceCodeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

        }

        public async Task<bool> ConvertForIngestionAsync(string filePath)
        {
            IEnumerable<CodeChunk> chunks = [];
            var fileExtension = Path.GetExtension(filePath);
            var scope = _scopeFactory.CreateScope();

            if (_settings.CSharpExtensions.Contains(fileExtension))
            {
                var csharpRoslynStrategy = scope.ServiceProvider.GetService<CSharpRoslynChunkingStrategy>();
                chunks = await csharpRoslynStrategy!.ChunkFileAsync(filePath);
                return true;
            }
            else if (_settings.TypeScriptExtensions.Contains(fileExtension))
            {
                var typescriptStrategy = scope.ServiceProvider.GetService<TypeScriptChunkingStrategy>();
                chunks = await typescriptStrategy!.ChunkFileAsync(filePath);
                return true;
            }

            return false;
        }
    }
}