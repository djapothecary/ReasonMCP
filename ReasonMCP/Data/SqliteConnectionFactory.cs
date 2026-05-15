using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Data
{
    public class SqliteConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(IOptions<StorageConfig> config)
        {
            _connectionString = $"Data Source={config.Value.VectorDbPath}";
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}