using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces.IEnrichment;

namespace ReasonMCP.Models
{
    public class ReferenceVectorModel : IEnrichmentVectorModel
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData]
        public string Source { get; set; } = string.Empty;

        [VectorStoreData]
        public string Topic { get; set; } = string.Empty;

        [VectorStoreData]
        public string HeaderContext { get; set; } = string.Empty;

        [VectorStoreData]
        public int ChunkIndex { get; set; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public string Content { get; set; } = string.Empty;

        [VectorStoreData(IsFullTextIndexed = true)]
        public string Text { get; set; } = string.Empty;

        [VectorStoreData(IsIndexed = true)]
        public string FilePath { get; set; } = string.Empty;

        [VectorStoreData(IsIndexed = true)]
        public string NodeUri { get; set; } = string.Empty;    // e.g., "MethodDeclaration", "ClassDeclaration"

        [VectorStoreData]
        public string NodeType { get; set; } = string.Empty;   // e.g., "ReasonMCP.Services.MyService.MyMethod"

        [VectorStoreData]
        public int StartLine { get; set; }

        [VectorStoreData]
        public int EndLine { get; set; }

        [VectorStoreData]
        public string LastModified { get; set; } = string.Empty;

        [VectorStoreData]
        public string GeneratedDate { get; set; } = string.Empty;

        [VectorStoreData]
        public string Version { get; set; } = string.Empty;

        [VectorStoreData]
        public string MetadataJson { get; set; } = string.Empty;

        public Dictionary<string, string>? Metadata { get; set; }

        //  MUST MATCH embedding model
        //  this is for nomic
        [VectorStoreVector(Dimensions: 768)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}