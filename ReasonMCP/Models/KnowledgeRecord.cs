using Microsoft.Extensions.VectorData;

namespace ReasonMCP.Models
{
    public class KnowledgeRecord
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData(IsFullTextIndexed = true)]
        public string? Text { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? Topic { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? Source { get; set; }

        [VectorStoreData]
        public string? HeaderContext { get; set; }

        [VectorStoreData]
        public int ChunkIndex { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? Version { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? GeneratedDate { get; set; }

        //  MUST MATCH embedding model
        //  this is for nomic
        [VectorStoreVector(Dimensions: 768)]
        public ReadOnlyMemory<float> Vector { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }
}