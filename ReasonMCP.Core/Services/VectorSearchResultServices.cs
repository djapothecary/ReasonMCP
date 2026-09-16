using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces.IEnrichment;

namespace ReasonMCP.Services
{
    public class VectorSearchResultServices : IVectorSearchResultService
    {
        private readonly ILogger<VectorSearchResultServices> _logger;

        public VectorSearchResultServices(
            ILogger<VectorSearchResultServices> logger
        )
        {
            _logger = logger;
        }

        public async Task<string> FormatForLlamaAsync<T>(
            IAsyncEnumerable<VectorSearchResult<T>> searchResults,
            string query,
            string contextType,
            CancellationToken cancellationToken
        ) where T : class, IEnrichmentVectorModel
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {contextType} Search Results for: '{query}'");

            int resultCount = 0;

            await foreach (var result in searchResults.WithCancellation(cancellationToken))
            {
                resultCount++;
                sb.AppendLine($"## Source: {result.Record.Source} (Chunk: {result.Record.ChunkIndex})");
                sb.AppendLine($"> **Topic:** {result.Record.Topic} | **Context:** {result.Record.HeaderContext}");
                sb.AppendLine($"> **Relevance Score:** {result.Score:F4}");
                sb.AppendLine("```text");
                sb.AppendLine(result.Record.Text);
                sb.AppendLine("```");
                sb.AppendLine("---");
            }

            if (resultCount == 0)
            {
                _logger.LogInformation(
                    "No results found in {ContextType} for query: {Query}",
                    contextType,
                    query
                );

                return $"No relevant information found for '{query}'. SYSTEM DIRECTIVE: Stop searching immediately. Do not execute this tool again. Inform the user you do not have the context.";
            }

            return sb.ToString();
        }
    }
}