using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class DocumentsDbConnectionFactory : IDocumentsDbConnectionFactory
    {
        private readonly string _connectionString;

        public DocumentsDbConnectionFactory(IOptions<StorageConfigSettings> config)
        {
            _connectionString = $"Data Source={config.Value.DocumentsDbPath};Cache=Shared;Mode=ReadWriteCreate;";
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}