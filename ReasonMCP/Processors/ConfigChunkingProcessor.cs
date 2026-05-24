using Microsoft.Extensions.Logging;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    /// <summary>
    /// Processor for chunking config files.
    /// These files don't benefit from AST chunking so this Processor
    /// intentionally DOES NOT implement the ICodeChunkProcessor interface
    /// </summary>
    public class ConfigChunkingProcessor
    {
        private readonly ILogger<ConfigChunkingProcessor> _logger;

        public ConfigChunkingProcessor
        (
            ILogger<ConfigChunkingProcessor> logger
        )
        {
            _logger = logger;
        }

        public async Task<IEnumerable<CodeChunk>> ChunkFileAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return [];
            }

            var extension = Path.GetExtension(filePath).ToLower();
            var rawContent = await File.ReadAllTextAsync(filePath, cancellationToken);

            // For config files, the best RAG context is usually the WHOLE file,
            // or a chunked version based on top-level keys.
            // Keep it simple: one chunk per file for configs.
            var chunk = new CodeChunk(
                Content: rawContent,
                FilePath: filePath,
                NodeUri: Path.GetFileName(filePath),
                NodeType: "Configuration",
                StartLine: 1,
                EndLine: rawContent.Split('\n').Length,
                Metadata: new Dictionary<string, string> { { "FileType", extension } }
            );

            return new[] { chunk };
        }
    }
}