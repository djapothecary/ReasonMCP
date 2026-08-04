using Microsoft.Data.Sqlite;

namespace ReasonMCP.Interfaces
{
    public interface IDocumentsDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}