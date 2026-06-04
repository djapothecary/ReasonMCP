namespace ReasonMCP.Configurations
{
    public class StorageConfigSettings
    {
        public string KnowledgeBaseRootDirectory { get; set; } = string.Empty;
        public string VectorDbPath { get; set; } = string.Empty;
        public string NewsLettersPath { get; set; } = string.Empty;
        public string ADRsPath { get; set; } = string.Empty;
        public string GeneralPath { get; set; } = string.Empty;
        public bool ClearOriginalFile { get; set; }
    }
}