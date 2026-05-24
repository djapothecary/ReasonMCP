using System.Text.Json.Serialization;

namespace ReasonMCP.Configurations
{
    public class KnowledgebaseScanSettings
    {
        public bool Enabled { get; set; }
        public string RootDirectory { get; set; } = string.Empty;
        public List<string> SubDirectories { get; set; } = [];
        public List<string> ExcludedDirectories { get; set; } = [];
        public List<string> ExcludeFilesContaining { get; set; } = [];
        public List<string> KnowledgeExtensions { get; set; } = [];

        //  Generated flat list
        [JsonIgnore]
        public IEnumerable<string> AllKnowledgeExtensions =>
            KnowledgeExtensions.Distinct(StringComparer.OrdinalIgnoreCase);
    }
}