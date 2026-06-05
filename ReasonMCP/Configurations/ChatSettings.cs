using DocumentFormat.OpenXml.Office.CoverPageProps;

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
        /// (e.g. ".json")        ///
        /// </summary>
        /// <value></value>
        public string HistoryFileExtension { get; set; } = string.Empty;

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
        /// The Reason Chat history directory
        /// currently un-used
        /// </summary>
        /// <value></value>
        public string ReasonHistoryDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The Reason Chat History filename
        /// </summary>
        /// <value></value>
        public string ReasonHistoryFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Current Reason chat context file
        /// this will be constructed with DateTime.UtcNow and HistoryFileExtension
        /// </summary>
        /// <value></value>
        public string ReasonCurrentContextFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Reason Chat History Summarization threshold value
        /// when the turn count exceeds this value, context summarization will occur
        /// </summary>
        /// <value></value>
        public int ReasonSummarizationThreshold { get; set; }

        /// <summary>
        /// The Reason Participant ID used to identify the agent in the extension "frontend"
        /// </summary>
        /// <value></value>
        public string ReasonParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// The Bella Chat history directory
        /// currently un-used
        /// </summary>
        /// <value></value>
        public string BellaHistoryDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The Bella Chat History filename
        /// </summary>
        /// <value></value>
        public string BellaHistoryFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Current Bella chat context filename
        /// this will be constructed with DateTime.UtcNow and HistoryFileExtension
        /// </summary>
        /// <value></value>
        public string BellaCurrentContextFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Bella Chat History Summarization threshold value
        /// when the turn count exceeds this value, context summarization will occur
        /// </summary>
        /// <value></value>
        public int BellaSummarizationThreshold { get; set; }

        /// <summary>
        /// The Bella Participant ID used to identify the agent in the extension "frontend"
        /// </summary>
        /// <value></value>
        public string BellaParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// The Plan Chat history directory
        /// currently un-used
        /// </summary>
        /// <value></value>
        public string PlanHistoryDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The Plan Chat History filename
        /// </summary>
        /// <value></value>
        public string PlanHistoryFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Plan current chat context filename
        /// this will be constructed with DateTime.UtcNow and HistoryFileExtension
        /// </summary>
        /// <value></value>
        public string PlanCurrentContextFilename { get; set; } = string.Empty;

        /// <summary>
        /// The Plan Chat History Summarization threshold value
        /// when the turn count exceeds this value, context summarization will occur
        /// </summary>
        /// <value></value>
        public int PlanSummarizationThreshold { get; set; }

        /// <summary>
        /// The Plan Participant ID used to identify the agent in the extension "frontend"
        /// </summary>
        /// <value></value>
        public string PlanParticipantId { get; set; } = string.Empty;
    }
}