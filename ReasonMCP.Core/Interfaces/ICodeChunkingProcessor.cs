using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    /// <summary>
    /// Processor for chunking C# source code into semantic AST-based elements.
    /// </summary>
    public interface ICodeChunkingProcessor
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
    }
}