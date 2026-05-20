using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReasonMCP.Data;

namespace ReasonMCP.Extensions
{
    public static class VectorDbExtensions
    {
        public static IHostApplicationBuilder AddReasonVectorDbService(
            this IHostApplicationBuilder builder
        )
        {
            //  1.  Register DatabaseInitializer
            builder.Services.AddTransient<DatabaseInitializer>();

            //  2.  Defer the DB creation until requested by the DI Container
            builder.Services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<SqliteConnection>>();

                var dbPath = config.GetValue<string>("StorageConfig:VectorDbPath")
                            ?? "ReasonContext.db";

                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //  Create ONE Singleton  connection, open it and load the extension
                var dataSource = $"Data Source={dbPath}";
                var masterConnection = new SqliteConnection(dataSource);
                masterConnection.Open();
                masterConnection.EnableExtensions(true);

                //  Attempt to load the native binary
                try
                {
                    masterConnection.LoadExtension("vec0", "sqlite3_vec_init");
                    logger.LogTrace("Loaded sqlite-vec extension successfully.");
                }
                catch (Exception ex1)
                {
                    try
                    {
                        //  Fallback for alternate binary versions
                        masterConnection.LoadExtension("vec0", "sqlite3_vec0_init");
                        logger.LogTrace("Loaded fallback sqlite-vec0 extension successfully.");
                    }
                    catch (Exception ex2)
                    {
                        //  3.  FAIL LOUDLY! If the AI can't load the vector engine, the app must crash!
                        logger.LogCritical(ex2, "[VECTOR_DB_ERROR] Critical failure loading sqlite-vec extension.");

                        throw new InvalidOperationException("Could not initialize vector database extensions.", ex2);
                    }
                }

                return masterConnection;
            });

            return builder;
        }
    }
}