using Microsoft.Data.Sqlite;

namespace ReasonMCP.Core.Interfaces
{
    public interface IIngestionQueueDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}