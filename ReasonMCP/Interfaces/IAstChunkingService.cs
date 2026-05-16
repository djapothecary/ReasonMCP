using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    /// <summary>
    /// Service for chunking C# source code into semantic AST-based elements.
    /// </summary>
    public interface IAstChunkingService
    {
        /// <summary>
        /// Chunks a C# source file asynchronously into semantic code chunks.
        /// </summary>
        /// <param name="filePath">The path to the C# source file.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>An enumerable of code chunks extracted from the file.</returns>
        Task<IEnumerable<CodeChunk>> ChunkFileAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Chunks C# source code from a string into semantic code chunks.
        /// </summary>
        /// <param name="sourceCode">The C# source code as a string.</param>
        /// <param name="filePath">The virtual file path (for metadata purposes).</param>
        /// <returns>An enumerable of code chunks extracted from the source code.</returns>
        IEnumerable<CodeChunk> ChunkSourceCode(
            string sourceCode,
            string filePath
        );
    }
}