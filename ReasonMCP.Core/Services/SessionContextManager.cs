using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;

namespace ReasonMCP.Services
{
    /// <summary>
    /// Manages per-session context file paths for chat strategies.
    /// Each agent strategy instance gets its own session context file that persists for the session.
    /// </summary>
    public class SessionContextManager
    {
        private readonly string _rootDirectory;
        private readonly string _historyDirectory;
        private readonly string _summaryDirectory;
        private readonly string _fileExtension;
        private readonly ILogger<SessionContextManager> _logger;

        public SessionContextManager
        (
            IOptionsMonitor<ChatSettings> options,
            ILogger<SessionContextManager> logger
        )
        {
            var settings = options.CurrentValue;
            _rootDirectory = settings.RootDirectory;
            _historyDirectory = settings.ChatHistoryDirectory;
            _summaryDirectory = settings.SummaryDirectoryPath;
            _fileExtension = settings.FileExtension;
            _logger = logger;
        }

        /// <summary>
        /// Generates the deterministic path for the current chat session.
        /// </summary>
        public string GetCurrentContextFilePath(string agentId, string sessionId)
        {
            // Builds: "D:\Remote_Source\ReasonData\History\bella_context_a1b2c3d4.jsonl"
            // Note: Use Path.Combine in production so you don't worry about missing slashes!
            string fileName = $"{agentId}_context_{sessionId}{_fileExtension}";
            // return Path.Combine(_rootDirectory, _historyDirectory, fileName);
            return _rootDirectory + _historyDirectory + "\\" + agentId + "CurrentContext\\" + fileName;
        }

        public string GetSummaryFilePath(string agentId, string sessionId)
        {
            string fileName = $"{agentId}_summary_{sessionId}{_fileExtension}";
            return Path.Combine(_rootDirectory, _summaryDirectory, fileName);
        }
    }
}