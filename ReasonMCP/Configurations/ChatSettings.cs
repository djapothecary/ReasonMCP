namespace ReasonMCP.Configurations
{
    public class ChatSettings
    {
        public string RootDirectory { get; set; } = string.Empty;
        public string ChatHistoryDirectory { get; set; } = string.Empty;
        public string ChatHistoryFilename { get; set; } = string.Empty;
        public int ChatSummarizationThreshold { get; set; }
        public string AgentHistoryDirectory { get; set; } = string.Empty;
        public string AgentHistoryFilename { get; set; } = string.Empty;
        public int AgentSummarizationThreshold { get; set; }
        public string PlanHistoryDirectory { get; set; } = string.Empty;
        public string PlanHistoryFilename { get; set; } = string.Empty;
        public int PlanSummarizationThreshold { get; set; }
    }
}