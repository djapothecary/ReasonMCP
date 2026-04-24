using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReasonMcp.Extensions
{
    public static class ReasonVectorDbExtensions
    {
        public static async Task<IHostApplicationBuilder> AddReasonVectorDbService(
            this IHostApplicationBuilder builder,
            IConfiguration configuration
        )
        {
            var dbPath = configuration.GetValue<string>("StorageConfig:VectorDbPath");
            var directory = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            //  Create ONE Singleton connection, open it and load the extension
            var dataSource = $"Data Source={dbPath}";
            var masterConnection = new SqliteConnection(dataSource);
            masterConnection.Open();
            masterConnection.EnableExtensions(true);

            //  Attempt to load the native binary.  Ensure SqliteVec.Native.Windows is in your project.
            try
            {
                masterConnection.LoadExtension("vec0", "sqlite3_vec_init");
            }
            catch (Exception ex)
            {
                //  Fallback for some alternate versions of the binary
                try
                {
                    masterConnection.LoadExtension("vec0", "sqlite3_vec0_init");
                }
                catch
                {
                    //  failed fall back
                    return builder;
                }
            }

            //  Manually initialize the virtual table
            using (var cmd = masterConnection.CreateCommand())
            {
                //  We manually create the virtual table that is expected
                //  the Nomic-embedd-text model used has a dimension of 768
                cmd.CommandText = @"
                    CREATE VIRTUAL TABLE IF NOT EXISTS vec_CodebaseContext USINC vec0(
                        Id TEXT PRIMARY KEY,
                        vector FLOAT[768]
                    );";

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {

                }
            }

            builder.Services.AddSingleton(masterConnection);

            return builder;
        }
    }
}