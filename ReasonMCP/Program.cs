using System.Data;
using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReasonMCP.Configurations;
using ReasonMCP.Data;
using ReasonMCP.Endpoints;
using ReasonMCP.Extensions;
using ReasonMCP.Tools;
using ReasonMCP.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:5000");
// builder.WebHost.UseUrls("http://127.0.0.1:11434");

builder.Configuration.SetBasePath(@"C:\Source\ReasonMCP\ReasonMCP\SharedConfigurations")
    .AddJsonFile("agentTaskWorkerSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("chatSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("codebaseScanSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("documentScanSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("gatewaySettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("referenceScanSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("storageConfigSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("testingSettings.json", optional: false, reloadOnChange: true)
    .Build();

builder.Services.Configure<AgentTaskWorkerSettings>(builder.Configuration.GetSection("AgentTaskWorkerSettings"));
builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection("ChatSettings"));
builder.Services.Configure<CodebaseScanSettings>(builder.Configuration.GetSection("CodebaseScanSettings"));
builder.Services.Configure<DocumentScanSettings>(builder.Configuration.GetSection("DocumentsScanSettings"));
builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection("GatewaySettings"));
builder.Services.Configure<KnowledgebaseScanSettings>(builder.Configuration.GetSection("KnowledgebaseScanSettings"));
builder.Services.Configure<ReferenceScanSettings>(builder.Configuration.GetSection("ReferenceScanSettings"));
builder.Services.Configure<StorageConfigSettings>(builder.Configuration.GetSection("StorageConfigSettings"));
builder.Services.Configure<TestingSettings>(builder.Configuration.GetSection("TestingSettings"));


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
// TODO:    Feature: Add a file logger to save/read logs
// builder.Logging.AddFile("logs/reason-mcp.log");

//  Add DB Factories and Initializers
builder.AddDbInitializers();
builder.AddDbFactories();
builder.AddDocumentsVectorStore();
builder.AddReferenceVectorStore();
builder.AddCodebaseVectorStore();

//  Add services from extensions
builder.AddReasonOllamaService();
builder.AddChatCompletionService();
builder.AddMnemosyneSummaryService();
builder.AddReasonNomicEmbedService();
builder.AddIngestionQueueService();
builder.AddCodebaseVectorDbService();
builder.AddDocumentsVectorDbService();
builder.AddReferenceVectorDbService();
builder.AddStrategies();
builder.AddFileServices();
builder.AddCodeChunkingServices();
builder.AddAiGatewayService();

//  Agent Services and Strategies
builder.AddAgentChatStrategies();
builder.AddAgents();
builder.AddAgentServices();
builder.AddAIPluginsAndTools();

//  Enrichment Extensions
builder.AddWorkflows();
builder.AddEnrichmentServices();

//  testing AI Agent chat interception
// builder.Services.AddSingleton<IFunctionInvocationFilter, ChatInterceptor>();

//  Register the Background Services
//  Scanners for Codebase and Documents/Knowledge
builder.Services.AddHostedService<EnrichmentWorker>();
builder.Services.AddHostedService<AgentTaskWorker>();

builder.Services.AddSingleton(Channel.CreateUnbounded<object>());
//builder.Services.AddHostedService<AgentTaskWorker>();

// Add the MCP services: the transport to use and the tools to register.
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
webHost.MapGradingEndpoints();
// webHost.MapTroubleshootingEndpoints();

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
    var dbCodebaseInit = services.GetRequiredService<CodebaseVectorDbInitializer>();
    await dbCodebaseInit.InitializeCodebaseDbAsync();

    var dbDocumentsInit = services.GetRequiredService<DocumentsVectorDbInitializer>();
    await dbDocumentsInit.InitializeDocumentsDbAsync();

    var dbReferenceInit = services.GetRequiredService<ReferenceVectorDbInitializer>();
    await dbReferenceInit.InitializeReferenceDbAsync();

    var dbIngestionQueueInit = services.GetRequiredService<IngestionQueueDbInitializer>();
    await dbIngestionQueueInit.InitializeIngestionQueueDbAsync();

    logger.LogInformation("Ingestion complete. Start MCP Server loop ...");
}

await webHost.RunAsync();
