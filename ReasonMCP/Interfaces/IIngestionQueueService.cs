using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IIngestionQueueService
    {
        /// <summary>
        /// Inserts the file into the queue. If it already exists, updates it ONLY IF
        /// the LastModified date is newer than what is in the database.
        /// </summary>
        Task UpsertToQueueAsync(string filePath, string TargetStore, DateTime lastModified, CancellationToken cancellationToken = default);

        /// <summary>
        /// Claims the next 'Pending' file and sets its status to 'Processing' to prevent race conditions.
        /// </summary>
        Task<FileIngestionRecord?> DequeueNextFileAsync(string targetStore, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks the file as successfully vector-embedded and complete.
        /// </summary>
        Task MarkCompleteAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks the file as failed, increments retry count, and saves the error message.
        /// </summary>
        Task MarkFailedAsync(string filePath, string errorMessage, CancellationToken cancellationToken = default);
    }
}