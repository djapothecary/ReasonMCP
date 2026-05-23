namespace ReasonMCP.Configurations
{
    public class GatewaySettings
    {
        public string Url { get; set; } = "http://127.0.0.1";
        public string Port { get; set; } = "5000";
        public int ChatSummarizationThreshold { get; set; } = 10;
    }
}