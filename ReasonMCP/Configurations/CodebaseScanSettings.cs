using System.Text.Json.Serialization;
using ReverseMarkdown;

namespace ReasonMCP.Configurations
{
    public class CodebaseScanSettings
    {
        public bool Enabled { get; set; } = false;
        public bool RunFileScan { get; set; } = false;
        public bool ProcessFiles { get; set; } = false;
        public bool GenerateEmbeddings { get; set; } = false;
        public List<string> RootDirectories { get; set; } = [];
        public List<string> SubDirectories { get; set; } = [];
        public List<string> ExcludedDirectories { get; set; } = [];
        public List<string> ExcludeFilesContaining { get; set; } = [];
        public List<string> CSharpExtensions { get; set; } = [];
        public List<string> TypeScriptExtensions { get; set; } = [];
        public List<string> SourceCodeExtensions { get; set; } = [];
        public List<string> MarkupExtensions { get; set; } = [];
        public List<string> ConfigExtensions { get; set; } = [];
        public List<string> SqlExtensions { get; set; } = [];

        //  Generated Flat list
        //  This is intended for future use or display/reporting purposes
        //  Use [JsonIgnore] so the config binder doesn't try to map this from the JSON file
        [JsonIgnore]
        public IEnumerable<string> AllTargetExtensions =>
            SourceCodeExtensions
            .Concat(MarkupExtensions)
            .Concat(ConfigExtensions)
            .Concat(SqlExtensions)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    }
}