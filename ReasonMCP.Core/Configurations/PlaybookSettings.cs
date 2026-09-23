namespace ReasonMCP.Core.Configurations
{
    public class PlaybookSettings
    {
        public string RootDirectory { get; set; } = string.Empty;
        public string FileExtension { get; set; } = ".jsonl";
        public string PlaybookHistoryDirectory { get; set; } = string.Empty;
        public string DotReasonDirectory { get; set; } = string.Empty;
        public string ReasonMCPRootDirectory { get; set; } = string.Empty;
        public string PlaybooksDirectory { get; set; } = string.Empty;
    }
}