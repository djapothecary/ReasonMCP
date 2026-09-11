namespace ReasonMCP.Configurations
{
    public class FileSystemSecuritySettings
    {
        public bool Enabled { get; set; }
        public List<string> AllowedRootDirectories { get; set; } = [];
    }
}