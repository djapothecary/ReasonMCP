using Microsoft.Data.Sqlite;

namespace ReasonMCP.Interfaces
{
    public interface IReferenceDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}