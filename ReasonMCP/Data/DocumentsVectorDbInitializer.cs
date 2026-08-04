using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class DocumentsVectorDbInitializer
    {
        private readonly IDocumentsDbConnectionFactory _connectionFactory;
        private ILogger<DocumentsVectorDbInitializer> _logger;

        public DocumentsVectorDbInitializer(
            IDocumentsDbConnectionFactory factory,
            ILogger<DocumentsVectorDbInitializer> logger
        )
        {
            _connectionFactory = factory;
            _logger = logger;
        }

        public async Task InitializeDocumentsDbAsync()
        {
            _logger.LogInformation("Initializing SQLite Databases ...");

            try
            {
                using var connection = _connectionFactory.CreateConnection();
                connection.Open();
                using var cmd = connection.CreateCommand();

                //  1.  Create the Documents Vector Table
                cmd.CommandText = @"
                    CREATE VIRTUAL TABLE IF NOT EXISTS vec_Documents USING vec0(
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