using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class IngestionQueueDbConnectionFactory : IIngestionQueueDbConnectionFactory
    {
        private readonly string _connectionString;

        public IngestionQueueDbConnectionFactory(
            IOptions<StorageConfigSettings> config
        )
        {
            _connectionString = $"Data Source={config.Value.IngestionQueueDbPath};Cache=Shared;Mode=ReadWriteCreate;";
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}