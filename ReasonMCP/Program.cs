using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ReasonMcp.Extensions;
using ReasonMCP.Extensions;
using ReasonMCP.Models;

var builder = Host.CreateApplicationBuilder(args);

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
await builder.AddReasonVectorDbService(builder.Configuration);

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<RandomNumberTools>();

using IHost host = builder.Build();

using (var scope = host.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Initializing ReasonMCP pre-flight ingestion ...");

    //  File scanner (orchestrator) will go here
    var kernel = services.GetRequiredService<Kernel>();

    logger.LogInformation("Ingestion complete. Start MCP Server loop ...");
}

await host.RunAsync();
