namespace ReasonMCP.Interfaces.IEnrichment
{
    public interface IEnrichmentVectorModel
    {
        string Id { get; }
        string Source { get; }
        string Topic { get; }
        string HeaderContext { get; }
        int ChunkIndex { get; }
        string Content { get; }
        string Text { get; }
        string FilePath { get; }
        string NodeUri { get; }
        string NodeType { get; }
        int StartLine { get; }
        int EndLine { get; }
        string LastModified { get; }
        string GeneratedDate { get; }
    }
}