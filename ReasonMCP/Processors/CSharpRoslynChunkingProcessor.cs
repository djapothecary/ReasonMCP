using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Walkers;

namespace ReasonMCP.Processors
{
    /// <summary>
    /// High-performance C# service using Roslyn for semantic code chunking.
    /// Extracts logical AST nodes (classes, interfaces, structs, enums, methods) for vector embedding.
    /// </summary>
    public class CSharpRoslynChunkingProcessor : ICodeChunkingProcessor
    {
        private readonly ILogger<CSharpRoslynChunkingProcessor> _logger;

        public CSharpRoslynChunkingProcessor
        (
            ILogger<CSharpRoslynChunkingProcessor> logger
        )
        {
            _logger = logger;
        }

        /// <summary>
        /// Chunks a C# source file asynchronously into semantic code chunks.
        /// </summary>
        public async Task<IEnumerable<CodeChunk>> ChunkFileAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return [];
                }

                var sourceCode = await File.ReadAllTextAsync(filePath, cancellationToken);
                return ChunkSourceCode(sourceCode, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error chunking file: {FilePath}", filePath);
                return [];
            }
        }

        /// <summary>
        /// Chunks C# source code from a string into semantic code chunks.
        /// </summary>
        public IEnumerable<CodeChunk> ChunkSourceCode(
            string sourceCode,
            string filePath
        )
        {
            try
            {
                var tree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = tree.GetCompilationUnitRoot();

                var chunks = new List<CodeChunk>();
                var walker = new AstChunkingWalker(filePath, sourceCode);
                walker.Visit(root);

                return walker.Chunks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing source code for file: {FilePath}", filePath);
                return [];
            }
        }
    }
}