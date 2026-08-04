using Microsoft.Extensions.VectorData;

namespace ReasonMCP.Models
{
    public class CodebaseVectorModel
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData]
        public string? Source { get; set; }

        [VectorStoreData]
        public string? Topic { get; set; }

        [VectorStoreData]
        public string? HeaderContext { get; set; }

        [VectorStoreData]
        public int ChunkIndex { get; set; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public string? Content { get; set; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public string? Text { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? FilePath { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string? NodeUri { get; set; }    // e.g., "MethodDeclaration", "ClassDeclaration"

        [VectorStoreData]
        public string? NodeType { get; set; }   // e.g., "ReasonMCP.Services.MyService.MyMethod"

        [VectorStoreData]
        public int? StartLine { get; set; }

        [VectorStoreData]
        public int? EndLine { get; set; }

        [VectorStoreData]
        public string? LastModified { get; set; }

        [VectorStoreData]
        public string? GeneratedDate { get; set; }

        [VectorStoreData]
        public string? Version { get; set; }

        [VectorStoreData]
        public string? MetadataJson { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }

        //  MUST MATCH embedding model
        //  this is for nomic
        [VectorStoreVector(Dimensions: 768)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}