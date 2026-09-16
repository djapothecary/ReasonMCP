using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    /// <summary>
    /// Processor for chunking config files.
    /// </summary>
    public class ConfigChunkingProcessor : ICodeChunkingProcessor
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
            var lines = rawContent.Split('\n');

            //  Nomic embed-text allows ~8192 tokens, 500 lines is extremely safe.
            const int MaxLinesPerChunk = 500;
            var chunks = new List<CodeChunk>();

            //  If it's a normal sized config file, returh the whole thing
            if (lines.Length <= MaxLinesPerChunk)
            {
                chunks.Add(new CodeChunk(
                    Content: rawContent,
                    FilePath: filePath,
                    NodeUri: Path.GetFileName(filePath),
                    NodeType: "Configuration",
                    StartLine: 1,
                    EndLine: lines.Length,
                    Metadata: new Dictionary<string, string> { { "FileType", extension } }
                ));

                return chunks;
            }

            //  Safety valve: It's a massive config file.  Slice it by line limits
            int currentLine = 0;
            int partNumber = 1;

            while (currentLine < lines.Length)
            {
                var chunkLines = lines.Skip(currentLine).Take(MaxLinesPerChunk);
                var chunkContent = string.Join('\n', chunkLines);

                chunks.Add(new CodeChunk(
                    Content: chunkContent,
                    FilePath: filePath,
                    //  Append _Part1, _Part2 to the URI so Reason knows it's fragmented
                    NodeUri: $"{Path.GetFileName(filePath)}+Part(partNumer)",
                    NodeType: "Configuration",
                    StartLine: currentLine + 1,
                    EndLine: Math.Min(currentLine + MaxLinesPerChunk, lines.Length),
                    Metadata: new Dictionary<string, string> { { "FileType", extension } }
                ));

                currentLine += MaxLinesPerChunk;
                partNumber++;
            }

            return chunks;
        }
    }
}