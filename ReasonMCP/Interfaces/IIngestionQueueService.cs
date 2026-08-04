using ReasonMCP.Models;

namespace ReasonMCP.Interfaces
{
    public interface IIngestionQueueService
    {
        /// <summary>
        /// Inserts the file into the queue. If it already exists, updates it ONLY IF
        /// the LastModified date is newer than what is in the database.
        /// </summary>
        Task UpsertToQueueAsync(
            string filePath,
            string TargetStore,
            DateTime lastModified,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets the next file to Embed by TargetStore
        /// </summary>
        /// <param name="targetStore"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FileIngestionRecord?> DequeueNextFileToEmbedAsync(
            string targetStore,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Claims the next 'Pending' file and sets its status to 'Processing' to prevent race conditions.
        /// </summary>
        Task<FileIngestionRecord?> DequeueNextFileAsync(
            string targetStore,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Marks the file as successfully vector-embedded and complete.
        /// </summary>
        Task MarkConversionCompleteAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Marks the file as successfully vector-embedded and complete.
        /// </summary>
        Task MarkCompleteAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Marks the file as Conversion failed, increments retry count, and saves the error message.
        /// </summary>
        Task MarkConversionFailedAsync(
            string filePath,
            string errorMessage,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Marks the file as failed Ingestion, increments retry count, and saves the error message.
        /// </summary>
        Task MarkIngestionFailedAsync(
            string filePath,
            string errorMessage,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Marks the file as failed due to an exception, increments retry count, and saves the error message.
        /// </summary>
        Task MarkFailedExceptionAsync(
            string filePath,
            string errorMessage,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Returns the count of files that have been successfully ingested
        /// </summary>
        /// <param name="targetStore"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> GetCountIngestedRecordsAsync(
            string targetStore,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Returns the count of the filest that have been successfully converted
        /// </summary>
        /// <param name="targetStore"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> GetCountConvertedRecordsAsync(
            string targetStore,
            CancellationToken cancellationToken = default
        );
    }
}