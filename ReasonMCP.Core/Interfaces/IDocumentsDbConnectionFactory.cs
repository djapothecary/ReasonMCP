using Microsoft.Data.Sqlite;

namespace ReasonMCP.Core.Interfaces
{
    public interface IDocumentsDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}