namespace ReasonMCP.Configurations
{
    public class DocumentScanSettings
    {
        public bool Enabled { get; set; } = false;
        public bool RunFileScan { get; set; } = false;
        public bool ProcessFiles { get; set; } = false;
        public bool GenerateEmbeddings { get; set; } = false;
        public List<string> RootDirectories { get; set; } = [];
        public List<string> SubDirectories { get; set; } = [];
        public List<string> ExcludedDirectories { get; set; } = [];
    }
}