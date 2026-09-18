using ReasonMCP.Core.Models;

namespace ReasonMCP.Enrichment.Interfaces
{
    public interface IMetadataEnrichmentUtility
    {
        Task<List<RagObject>> EnrichChunksAsync(IEnumerable<string> chunks, string sourceName);
    }
}