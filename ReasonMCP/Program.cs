using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReasonMCP.Data;
using ReasonMCP.Extensions;
using ReasonMCP.Models;
using ReasonMCP.Tools;
using ReasonMCP.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.Configure<StorageConfig>(builder.Configuration.GetSection("StorageConfig"));

//  Clear default providers so that nothing leaks accidently leaks to STDOUT
builder.Logging.ClearProviders();

//  Add the console forcing STDERR for everything
//  Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
// TODO:    Feature: Add a file logger to save/read logs without relying on Continue.dev's debug window
// builder.Logging.AddFile("logs/reason-mcp.log");

//  Add services from extensions
builder.AddReasonOllamaService();
builder.AddReasonNomicEmbedService();
builder.AddReasonVectorDbService();
builder.AddReasonVectorStore();
builder.AddFileServices();

//  Register the Background Service
builder.Services.AddHostedService<DocumentProcessingWorker>();

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<KnowledgeSearchTool>();

using WebApplication webHost = builder.Build();

using (var scope = webHost.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Initializing ReasonMCP pre-flight ingestion ...");

    //  This gaurantees the vec0 tables exist BEFORE the background worker wakes up!
    var dbInit = services.GetRequiredService<DatabaseInitializer>();
    await dbInit.InitializeDatabaseAsync();

    logger.LogInformation("Ingestion complete. Start MCP Server loop ...");
}

await webHost.RunAsync();
