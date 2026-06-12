namespace ReasonMCP.Configurations
{
    public class AgentFeatureSettings
    {
        /// <summary>
        /// The  Chat history directory
        /// currently un-used
        /// </summary>
        /// <value></value>
        public string HistoryDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The  Chat History filename
        /// </summary>
        /// <value></value>
        public string HistoryFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Current  chat context filename
        /// this will be constructed with DateTime.UtcNow and HistoryFileExtension
        /// </summary>
        /// <value></value>
        public string CurrentContextFilename { get; set; } = string.Empty;

        /// <summary>
        /// The  Participant ID used to identify the agent in the extension "frontend"
        /// </summary>
        /// <value></value>
        public string ParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// The path for the  Agent's YAML profile path
        /// </summary>
        /// <value></value>
        public string AgentProfilePath { get; set; } = string.Empty;
    }
}