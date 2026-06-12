namespace ReasonMCP.Configurations
{
    public class ChatSettings
    {
        /// <summary>
        /// The root ("parent") directory for all chat history files
        /// </summary>
        /// <value></value>
        public string RootDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The file extension to be used for all chat history files
        /// (e.g. ".json")
        /// </summary>
        /// <value></value>
        public string FileExtension { get; set; } = string.Empty;

        /// <summary>
        /// Chat history directory
        /// currently un-used
        /// </summary>
        /// <value></value>
        public string ChatHistoryDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The Chat History filename
        /// this will be constructed with DateTime.UtcNow and HistoryFileExtension
        /// </summary>
        /// <value></value>
        public string ChatHistoryFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Chat History Summarization threshold value
        /// when the turn count exceeds this value, context summarization will occur
        /// </summary>
        /// <value></value>
        public int ChatSummarizationThreshold { get; set; }

        /// <summary>
        /// The Summaries Directory path
        /// </summary>
        /// <value></value>
        public string SummaryDirectoryPath { get; set; } = string.Empty;

        /// <summary>
        /// Sets if prompt logging is turned on/off
        /// </summary>
        /// <value></value>
        public bool EnablePromptLogging { get; set; }

        /// <summary>
        /// The path to the Prompt logs
        /// </summary>
        /// <value></value>
        public string PromptLogPath { get; set; } = string.Empty;

        public Dictionary<string, AgentFeatureSettings> Agents { get; set; } = [];
    }
}