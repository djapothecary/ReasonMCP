using System.Text.Json.Serialization;

namespace ReasonMCP.Configurations
{
    public class ReferenceScanSettings
    {
        public bool Enabled { get; set; } = false;
        public bool RunFileScan { get; set; } = false;
        public bool ProcessFiles { get; set; } = false;
        public bool GenerateEmbeddings { get; set; } = false;
        public bool WriteConvertedOutput { get; set; } = false;
        public List<string> RootDirectories { get; set; } = [];
        public List<string> SubDirectories { get; set; } = [];
        public List<string> ExcludedDirectories { get; set; } = [];
        public List<string> ExcludeFilesContaining { get; set; } = [];
        public List<string> ReferenceExtensions { get; set; } = [];

        //  Generated flat list
        [JsonIgnore]
        public IEnumerable<string> AllReferenceExtensions =>
            ReferenceExtensions.Distinct(StringComparer.OrdinalIgnoreCase);
    }
}