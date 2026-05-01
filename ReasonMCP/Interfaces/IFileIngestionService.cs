using Microsoft.Extensions.AI;
using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IFileIngestionService
    {
        Task<bool> IngestSingleEnrichedObjectAsync(
            RagObject ragObject,
            IEmbeddingGenerator<string, Embedding<float>> embeddgingGenerator,
            CancellationToken cancellationToken
        );
    }
}