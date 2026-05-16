namespace ReasonMCP.Records
{
    /// <summary>
    /// Represents a semantic chunk of C# source code extracted from an AST.
    /// </summary>
    public record CodeChunk(
        /// <summary>
        /// The actual source code content of this chunk.
        /// </summary>
        string Content,

        /// <summary>
        /// The file path where this chunk originated.
        /// </summary>
        string FilePath,

        /// <summary>
        /// Fully qualified node URI (e.g., "Namespace.Class.Method").
        /// </summary>
        string NodeUri,

        /// <summary>
        /// The type of AST node (e.g., "MethodDeclaration", "ClassDeclaration").
        /// </summary>
        string NodeType,

        /// <summary>
        /// The starting line number of this chunk in the source file.
        /// </summary>
        int StartLine,

        /// <summary>
        /// The ending line number of this chunk in the source file.
        /// </summary>
        int EndLine,

        /// <summary>
        /// Additional metadata for enrichment (e.g., access modifiers, parent class, return type).
        /// </summary>
        Dictionary<string, string> Metadata
    );
}