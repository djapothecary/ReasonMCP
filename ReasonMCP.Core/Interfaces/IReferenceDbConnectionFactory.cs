using Microsoft.Data.Sqlite;

namespace ReasonMCP.Core.Interfaces
{
    public interface IReferenceDbConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}