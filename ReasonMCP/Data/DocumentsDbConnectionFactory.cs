using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class DocumentsDbConnectionFactory : IDocumentsDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger<DocumentsDbConnectionFactory> _logger;

        public DocumentsDbConnectionFactory(
            IOptions<StorageConfigSettings> config,
            ILogger<DocumentsDbConnectionFactory> logger
        )
        {
            _connectionString = $"Data Source={config.Value.DocumentsDbPath};Mode=ReadWriteCreate;";
            _logger = logger;
        }

        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);

            //  1.  Open the connection before loading the extension
            connection.Open();
            connection.EnableExtensions(true);

            // 1a. Build the absolute path to the native DLL inside the runtimes folder
            // (If you eventually deploy to Linux/Mac, you can add a quick OS check here,
            // but for your Windows workstation, this is perfect).
            string extensionPath = Path.Combine(
                AppContext.BaseDirectory,
                "vec0.dll"); // Include the .dll extension for absolute paths!

            //  2.  Load the vector math module into this specific connection
            try
            {
                connection.LoadExtension(extensionPath, "sqlite3_vec_init");
            }
            catch (Exception ex1)
            {
                try
                {
                    connection.LoadExtension(extensionPath, "sqlite3_vec0_init");
                }
                catch (Exception ex2)
                {
                    _logger.LogCritical(ex2, "Failed to load sqlite-vec extension into connection.");
                    throw new InvalidOperationException("Vector DB extension failed to load.", ex2);
                }
            }

            return connection;
        }
    }
}