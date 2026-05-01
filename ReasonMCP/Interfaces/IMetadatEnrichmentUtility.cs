using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IMetadataEnrichmentUtility
    {
        Task<List<RagObject>> EnrichChunksAsync(IEnumerable<string> chunks, string sourceName);
    }
}