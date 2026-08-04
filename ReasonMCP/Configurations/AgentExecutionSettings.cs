namespace ReasonMCP.Configurations
{
    public class AgentExecutionSettings
    {
        public string ModelId { get; set; } = "Reason";
        public string ServiceId { get; set; } = "Alpaca";
        public float Temperature { get; set; } = 0.7f;
        public float TopP { get; set; } = 0.9f;
        public int MaxTokens { get; set; } = 4096;
    }
}