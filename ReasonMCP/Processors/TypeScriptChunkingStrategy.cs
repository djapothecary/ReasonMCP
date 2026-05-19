using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    /// <summary>
    /// Chunking strategy for JS/TS/JSX/TSX files.
    /// Uses a semantic brace-counting parser to extract functions and classes cleanly.
    /// </summary>
    public partial class TypeScriptChunkingStrategy : ICodeChunkingStrategy
    {
        private readonly ILogger<TypeScriptChunkingStrategy> _logger;

        // Regex to match function, class, and arrow function declarations
        [GeneratedRegex(@"(?:export\s+)?(?:async\s+)?(?:function|class)\s+(?<name>[a-zA-Z0-9_]+)\s*[\(\{]|(?:const|let|var)\s+(?<name>[a-zA-Z0-9_]+)\s*=\s*(?:async\s*)?(?:\([^\)]*\)|[a-zA-Z0-9_]+)\s*=>\s*\{")]
        private partial Regex TsDeclarationRegex();

        public TypeScriptChunkingStrategy(ILogger<TypeScriptChunkingStrategy> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<CodeChunk>> ChunkFileAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            var sourceCode = await File.ReadAllTextAsync(filePath, cancellationToken);
            var chunks = new List<CodeChunk>();
            var lines = sourceCode.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var match = TsDeclarationRegex().Match(line);

                if (match.Success && line.Contains('{'))
                {
                    string nodeName = match.Groups["name"].Value;
                    var (blockContent, endLine) = ExtractBraceBlock(lines, i);

                    var metadata = new Dictionary<string, string>
                    {
                        { "Language", Path.GetExtension(filePath).TrimStart('.') },
                        { "NodeType", "JS/TS Block" }
                    };

                    chunks.Add(new CodeChunk(
                        Content: blockContent,
                        FilePath: filePath,
                        NodeUri: nodeName, // In a deeper implementation, you'd track scope here too
                        NodeType: "Block",
                        StartLine: i + 1,
                        EndLine: endLine + 1,
                        Metadata: metadata
                    ));

                    // Skip the parser ahead so we don't overlap chunks inside this function
                    i = endLine;
                }
            }

            return chunks;
        }

        /// <summary>
        /// Reads forward through the array of lines, counting open and close braces
        /// to extract a complete syntactic block without cutting it in half.
        /// </summary>
        private (string content, int endLine) ExtractBraceBlock(
            string[] lines,
            int startLine
        )
        {
            int openBraces = 0;
            bool insideBlock = false;
            var blockContent = new List<string>();

            for (int i = startLine; i < lines.Length; i++)
            {
                string line = lines[i];
                blockContent.Add(line);

                //  Count braces
                openBraces += line.Count(c => c == '{');
                openBraces -= line.Count(c => c == '}');

                if (openBraces > 0)
                {
                    insideBlock = true;
                }

                //  If we entered the block and the brace count returns 0,
                //  then the block is done
                if (insideBlock && openBraces == 0)
                {
                    return (string.Join('\n', blockContent).Trim(), i);
                }
            }

            //  Fallback if the file ends weirdly
            return (string.Join('\n', blockContent).Trim(), lines.Length - 1);
        }

        /// <summary>
        /// Intentionally not implemented for JS/TS chunking
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public IEnumerable<CodeChunk> ChunkSourceCode(string sourceCode, string filePath)
        {
            throw new NotImplementedException();
        }
    }
}