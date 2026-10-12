namespace ReasonMCP.Core.Configurations
{
    public class EscapeHatchSettings
    {
        // default to true, user MUST INTENTIONALLY turn this off!
        public bool Enabled { get; set; } = true;

        // FORCING a named directory
        public string EscapeHatchPath { get; set; } = "C:\\Source\\EscapeHatch\\";
    }
}