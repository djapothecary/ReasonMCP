using Microsoft.Data.Sqlite;

namespace ReasonMCP.Interfaces
{
    public interface IIngestionQueueDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}