using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class IngestionQueueDbInitializer
    {
        private readonly IIngestionQueueDbConnectionFactory _connectionFactory;
        private readonly ILogger<IngestionQueueDbInitializer> _logger;

        public IngestionQueueDbInitializer(
            IIngestionQueueDbConnectionFactory factory,
            ILogger<IngestionQueueDbInitializer> logger
        )
        {
            _connectionFactory = factory;
            _logger = logger;
        }

        public async Task InitializeIngestionQueueDbAsync()
        {
            _logger.LogInformation("Initializing SQLite Databases ...");

            try
            {
                using var connection = _connectionFactory.CreateConnection();
                connection.Open();
                using var cmd = connection.CreateCommand();



                //  1.  Create Standard table for FileIngestion queue
                _logger.LogInformation("Creating IngestionQueue for file processing ...");

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
                _logger.LogCritical(ex, "[DATABASE_INIT_ERROR] Failed to create virtual tables.");

                //  Re-throw to crash the app intentionally if the Database is dead.
                throw;
            }
        }
    }
}