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
            logger.LogInformation("Initializing SQLite Vector Databases ...");

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