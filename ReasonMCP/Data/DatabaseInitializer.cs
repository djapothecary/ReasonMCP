using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ReasonMCP.Data
{
    public class DatabaseInitializer(
        SqliteConnection masterConnection,
        ILogger<DatabaseInitializer> logger
    )
    {
        public async Task InitializeDatabaseAsync()
        {
            logger.LogInformation("Initializing SQLite Databases ...");

            try
            {
                using var cmd = masterConnection.CreateCommand();

                //  1.  Create the Documents Vector Table
                cmd.CommandText = @"
                    CREATE VIRTUAL TABLE IF NOT EXISTS vec_DocumentsContext USING vec0(
                        Id TEXT PRIMARY KEY,
                        vector FLOAT[768]
                    );";
                await cmd.ExecuteNonQueryAsync();

                //  2.  Create Codebase Vector Table
                cmd.CommandText = @"
                    CREATE VIRTUAL TABLE IF NOT EXISTS vec_CodebaseContext USING vec0(
                        Id TEXT PRIMARY KEY,
                        vector FLOAT[768]
                    );";
                await cmd.ExecuteNonQueryAsync();

                logger.LogInformation("Vector Databases initialized successfully.");

                //  3.  Create Standard table for FileIngestion queue
                logger.LogInformation("Creating IngestionQueue for file processing ...");

                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS IngestionQueue (
                        FilePath TEXT PRIMARY KEY,
                        TargetStore TEXT,
                        Status INTEGER NOT NULL,
                        LastModified TEXT NOT NULL,
                        RetryCount INTEGER DEFAULT 0,
                        ErrorMessage TEXT
                    );";
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                //  Safely print to stderr and prevent crashes from a polluted MCP stream
                logger.LogCritical(ex, "[DATABASE_INIT_ERROR] Failed to create virtual tables.");

                //  Re-throw to crash the app intentionally if the Database is dead.
                throw;
            }
        }
    }
}