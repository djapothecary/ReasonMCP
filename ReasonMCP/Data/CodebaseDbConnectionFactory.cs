using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Data
{
    public class CodebaseDbConnectionFactory : ICodebaseDbConnectionFactory
    {
        private readonly string _connectionString;

        public CodebaseDbConnectionFactory(IOptions<StorageConfigSettings> config)
        {
            _connectionString = $"Data Source={config.Value.CodebaseDbPath};Cache=Shared;Mode=ReadWriteCreate;";
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}