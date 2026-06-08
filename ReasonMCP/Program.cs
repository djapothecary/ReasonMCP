using System.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Configurations;
using ReasonMCP.Data;
using ReasonMCP.Endpoints;
using ReasonMCP.Extensions;
using ReasonMCP.Filters;
using ReasonMCP.Models;
using ReasonMCP.Tools;
using ReasonMCP.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:5000");
// builder.WebHost.UseUrls("http://127.0.0.1:11434");

builder.Services.Configure<StorageConfigSettings>(builder.Configuration.GetSection("StorageConfigSettings"));
builder.Services.Configure<TestingSettings>(builder.Configuration.GetSection("TestingSettings"));
builder.Services.Configure<CodebaseScanSettings>(builder.Configuration.GetSection("CodebaseScanSettings"));
builder.Services.Configure<KnowledgebaseScanSettings>(builder.Configuration.GetSection("KnowledgebaseScanSettings"));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

//  Clear default providers so that nothing leaks accidently leaks to STDOUT
builder.Logging.ClearProviders();

//  Add the console forcing STDERR for everything
//  Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole();
// TODO:    Feature: Add a file logger to save/read logs without relying on Continue.dev's debug window
// builder.Logging.AddFile("logs/reason-mcp.log");

//  Add services from extensions
builder.AddReasonOllamaService();
builder.AddChatCompletionService();
builder.AddReasonNomicEmbedService();
builder.AddReasonVectorDbService();
builder.AddReasonVectorStore();
builder.AddOrchestrators();
builder.AddStrategies();
builder.AddFileServices();
builder.AddIngestionQueueServices();
builder.AddCodeChunkingServices();
builder.AddAiGatewayService();

//  Agent Services and Strategies
builder.AddAgentChatStrategies();
builder.AddAgents();
builder.AddAgentServices();

//  testing AI Agent chat interception
builder.Services.AddSingleton<IFunctionInvocationFilter, ChatInterceptor>();

//  Register the Background Service

//  DEPRECATED: This have been replaced by the Seperate Workers
//builder.Services.AddHostedService<DocumentProcessingWorker>();

builder.Services.AddHostedService<KnowledgebaseScanWorker>();
builder.Services.AddHostedService<CodebaseScanWorker>();

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<DocumentContextSearchTool>();

builder.Services.AddKernel();

var webHost = builder.Build();
webHost.UseDeveloperExceptionPage();
webHost.MapHealthEndpoints();
// webHost.MapAiGatewayEndpoints();
// webHost.MapAiTestInterceptEndpoints();
webHost.MapReasonChatEndpoints();



webHost.MapGet("/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var endpoints = endpointSources.SelectMany(es => es.Endpoints).OfType<RouteEndpoint>();
    return endpoints.Select(e => new
    {
        Route = e.RoutePattern.RawText,
        Methods = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
    });
});


webHost.MapMcp();

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
