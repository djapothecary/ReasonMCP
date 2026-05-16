using Microsoft.Extensions.VectorData;

namespace ReasonMCP.Models
{
    public class CodebaseRecord
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData(IsFullTextIndexed = true)]
        public string? Context { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? FilePath { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? NodeUri { get; set; }

        [VectorStoreData]
        public string? NodeType { get; set; }

        [VectorStoreData]
        public int? StartLine { get; set; }

        [VectorStoreData]
        public int? EndLine { get; set; }

        [VectorStoreData]
        public string? LastModified { get; set; }

        [VectorStoreData]
        public Dictionary<string, string>? Metadata { get; set; }

        //  MUST MATCH embedding model
        //  this is for nomic
        [VectorStoreVector(Dimensions: 768)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}