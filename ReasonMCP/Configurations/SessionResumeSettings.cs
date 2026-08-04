using Azure.Core;

namespace ReasonMCP.Configurations
{
    public class SessionResumeSettings
    {
        public string SessionID { get; set; } = string.Empty;
        public string AgentId { get; set; } = "reason";
        public DateTime StartTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string SummaryFilepath { get; set; } = string.Empty;
        public string MasterHistoryFilepath { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}