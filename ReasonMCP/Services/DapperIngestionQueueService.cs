using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ReasonMCP.Enums;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Services
{
    public class DapperIngestionQueueService : IIngestionQueueService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DapperIngestionQueueService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task UpsertToQueueAsync(
            string filePath,
            string targetStore,
            DateTime lastModified,
            CancellationToken cancellationToken = default
        )
        {
            //  The SQLite "UPSERT": If the file exists, it only sets Status = Pending (0)
            //  If the new LastModifie date is strictly greater than the old one.
            const string sql = @"
                INSERT INTO IngestionQueue (
                    FilePath,
                    TargetStore,
                    Status,
                    LastModified,
                    RetryCount
                ) VALUES (
                    @FilePath,
                    @TargetStore,
                    @Status,
                    @LastModified,
                    0
                ) ON CONFLICT(FilePath) DO UPDATE SET
                    TargetStore = excluded.TargetStore,
                    Status = CASE WHEN excluded.LastModified > IngestionQueue.LastModified THEN 0 ELSE IngestionQueue.Status END,
                    LastModified = CASE WHEN excluded.LastModified > IngestionQueue.LastModified THEN excluded.LastModified ELSE IngestionQueue.LastModified END,
                    RetryCount = CASE WHEN excluded.LastModified > IngestionQueue.LastModified THEN 0 ELSE IngestionQueue.RetryCount END;";

            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(sql, new
            {
                FilePath = filePath,
                TargetStore = targetStore,
                Status = (int)IngestionStatus.Pending,
                LastModified = lastModified.ToString("O") //    Store as ISO 8601 string
            });
        }

        public async Task<FileIngestionRecord?> DequeueNextFileAsync(
            string targetStore,
            CancellationToken cancellationToken = default
        )
        {
            //  The Atomic Dequeue:  Finds the first pending record, sets it to Processing (1),
            //  and returns the updated record in a SINGLE, lock-free query.
            const string sql = @"
                UPDATE IngestionQueue
                SET Status = 1
                WHERE FilePath = (
                    SELECT FilePath FROM IngestionQueue
                    WHERE Status = 0 AND TargetStore = @TargetStore
                    ORDER BY LastModified DESC
                    LIMIT 1
                )
                RETURNING *;";

            using var connection = _connectionFactory.CreateConnection();

            //  QueryFirstOrDefaultAsync automatically maps the returned columns back to the model
            return await connection.QueryFirstOrDefaultAsync<FileIngestionRecord>(sql, new { TargetStore = targetStore });
        }

        public async Task MarkCompleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            const string sql = @"UPDATE IngestionQueue SET Status = 2 WHERE FilePath = @FilePath";
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { FilePath = filePath });
        }

        public async Task MarkFailedAsync(
            string filePath,
            string errorMessage,
            CancellationToken cancellationToken = default
        )
        {
            const string sql = @"
                UPDATE IngestionQueue
                SET Status = 3,
                    ErrorMessage = @ErrorMessage,
                    RetryCount = RetryCount + 1
                WHERE FilePath = @FilePath;";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { FilePath = filePath, ErrorMessage = errorMessage });
        }
    }
}