namespace ReasonMCP.Configurations
{
    public class StorageConfigSettings
    {
        public List<string> CodebaseRootDirectories { get; set; } = [];
        public List<string> DocumentsBaseRootDirectories { get; set; } = [];
        public List<string> ReferenceBaseRootDirectories { get; set; } = [];
        public string CodebaseDbPath { get; set; } = string.Empty;
        public string DocumentsDbPath { get; set; } = string.Empty;
        public string IngestionQueueDbPath { get; set; } = string.Empty;
        public string ReferenceDbPath { get; set; } = string.Empty;
        public string NewsLettersPath { get; set; } = string.Empty;
        public string ADRsPath { get; set; } = string.Empty;
        public string GeneralPath { get; set; } = string.Empty;
        public string DotNetDocs { get; set; } = string.Empty;
        public bool ClearOriginalFile { get; set; }
    }
}