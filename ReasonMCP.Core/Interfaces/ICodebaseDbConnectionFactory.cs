using Microsoft.Data.Sqlite;

namespace ReasonMCP.Core.Interfaces
{
    public interface ICodebaseDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}