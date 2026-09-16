using Microsoft.Data.Sqlite;

namespace ReasonMCP.Interfaces
{
    public interface ICodebaseDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}