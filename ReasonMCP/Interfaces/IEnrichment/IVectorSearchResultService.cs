using Microsoft.Extensions.VectorData;

namespace ReasonMCP.Interfaces.IEnrichment
{
    public interface IVectorSearchResultService
    {
        Task<string> FormatForLlamaAsync<T>(
            IAsyncEnumerable<VectorSearchResult<T>> searchResults,
            string query,
            string contextType,
            CancellationToken cancellationToken
        ) where T : class, IEnrichmentVectorModel;
    }
}