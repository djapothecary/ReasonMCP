using System;
using ReasonMCP.Enums;

namespace ReasonMCP.Models
{
    public class FileIngestionRecord
    {
        //  The unique identifier since a file path can only exist once
        public string FilePath { get; set; } = string.Empty;

        //  The vector store to save record to (e.g., "Codebase", "Documents", "ChatHistory")
        public string TargetStore { get; set; } = string.Empty;

        public IngestionStatus Status { get; set; } = IngestionStatus.Pending;

        //  Stored as ISO 8601 string or 'ticks' in SQLite, mapped to DateTime in C3
        public DateTime LastModified { get; set; }

        public int RetryCount { get; set; }

        public string? ErrorMessage { get; set; }
    }
}