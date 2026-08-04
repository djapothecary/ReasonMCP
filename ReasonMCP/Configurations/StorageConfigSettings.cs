namespace ReasonMCP.Configurations
{
    public class StorageConfigSettings
    {
        public string CodebaseRootDirectory { get; set; } = string.Empty;
        public string DocumentsBaseRootDirectory { get; set; } = string.Empty;
        public string ReferenceBaseRootDirectory { get; set; } = string.Empty;
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