using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class ReferenceVectorDbInitializer
    {
        private readonly IReferenceDbConnectionFactory _connectionFactory;
        private readonly ILogger<ReferenceVectorDbInitializer> _logger;

        public ReferenceVectorDbInitializer(
            IReferenceDbConnectionFactory factory,
            ILogger<ReferenceVectorDbInitializer> logger
        )
        {
            _connectionFactory = factory;
            _logger = logger;
        }

        public async Task InitializeReferenceDbAsync()
        {
            _logger.LogInformation("Initializing SQLite Databases ...");

            try
            {
                using var connection = _connectionFactory.CreateConnection();
                connection.Open();
                using var cmd = connection.CreateCommand();

                //  1.  Create Standard table for FileIngestion queue
                _logger.LogInformation("Creating Reference vector table for file processing ...");

                cmd.CommandText = @"
                    CREATE VIRTUAL TABLE IF NOT EXISTS vec_Reference USING vec0(
                        Id TEXT PRIMARY KEY,
                        vector FLOAT[768]
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