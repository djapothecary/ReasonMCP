using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class CodebaseVectorDbInitializer
    {
        private readonly ICodebaseDbConnectionFactory _connectionFactory;
        private readonly ILogger<CodebaseVectorDbInitializer> _logger;

        public CodebaseVectorDbInitializer(
            ICodebaseDbConnectionFactory factory,
            ILogger<CodebaseVectorDbInitializer> logger
        )
        {
            _connectionFactory = factory;
            _logger = logger;
        }

        public async Task InitializeCodebaseDbAsync()
        {
            _logger.LogInformation("Initializing SQLite Database ...");

            try
            {
                using var connection = _connectionFactory.CreateConnection();
                connection.Open();
                using var cmd = connection.CreateCommand();

                //  1.  Create Codebase Vector Table
                cmd.CommandText = @"
                    CREATE VIRTUAL TABLE IF NOT EXISTS vec_Codebase USING vec0(
                        Id TEXT PRIMARY KEY,
                        vector FLOAT[768]
                    );";
                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation("Vector Databases initialized successfully.");
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