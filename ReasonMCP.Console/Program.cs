using System.Data;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using ReasonMCP.Agents.Extensions;
using ReasonMCP.Agents.Tools;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Data;
using ReasonMCP.Core.Extensions;
using ReasonMCP.Core.Utilities;
using ReasonMCP.Core.Workers;
using ReasonMCP.Enrichment.Dispatchers;
using ReasonMCP.Enrichment.Extensions;
using ReasonMCP.Enrichment.Workers;
using Spectre.Console;
using Rule = Spectre.Console.Rule;

System.Console.OutputEncoding = Encoding.UTF8;
AnsiConsole.Write(new Rule("[yellow]ReasonMCP Neural Terminal (v10.0)[/]").RuleStyle("blue"));
AnsiConsole.MarkupLine($"{NeuralStyle.LogSys} Initializing ReasonMCP v10.0 Core...");
AnsiConsole.MarkupLine($"{NeuralStyle.LogSys} Loading Ollama endpoint: {NeuralStyle.Construct}http://127.0.0.1:11434{NeuralStyle.End}");
AnsiConsole.MarkupLine($"{NeuralStyle.Success} [bold]VEC0_STABLE:[/]{NeuralStyle.End} Neural memory synchronized (768 Dimensions).");

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(@"C:\Source\ReasonMCP\ReasonMCP.Core\SharedConfigurations")
    .AddJsonFile("agentTaskWorkerSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("chatSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("codebaseScanSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("documentScanSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("fileSystemSecuritySettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("gatewaySettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("referenceScanSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("storageConfigSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("testingSettings.json", optional: false, reloadOnChange: true)
    .Build();

builder.Services.Configure<AgentTaskWorkerSettings>(builder.Configuration.GetSection("AgentTaskWorkerSettings"));
builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection("ChatSettings"));
builder.Services.Configure<CodebaseScanSettings>(builder.Configuration.GetSection("CodebaseScanSettings"));
builder.Services.Configure<DocumentScanSettings>(builder.Configuration.GetSection("DocumentsScanSettings"));
builder.Services.Configure<FileSystemSecuritySettings>(builder.Configuration.GetSection("FileSystemSescuritySettings"));
builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection("GatewaySettings"));
builder.Services.Configure<KnowledgebaseScanSettings>(builder.Configuration.GetSection("KnowledgebaseScanSettings"));
builder.Services.Configure<ReferenceScanSettings>(builder.Configuration.GetSection("ReferenceScanSettings"));
builder.Services.Configure<StorageConfigSettings>(builder.Configuration.GetSection("StorageConfigSettings"));
builder.Services.Configure<TestingSettings>(builder.Configuration.GetSection("TestingSettings"));

//  Add Permisions/Policies
builder.AddAgentPermissonPolicies();

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
builder.AddEnrichmentUtilities();
// builder.AddStrategies();
builder.AddSecurityServices();
builder.AddSessionServices();
// builder.AddCodeChunkingServices();
builder.AddAiGatewayService();

//  Agent Services and Strategies
builder.AddAgentChatStrategies();
builder.AddAgents();
builder.AddAgentServices();
builder.AddAIPluginsAndTools();

//  Enrichment Extensions
builder.AddWorkflows();
builder.AddEnrichmentServices();
builder.AddEnrichmentDispatcher();

//  testing AI Agent chat interception
// builder.Services.AddSingleton<IFunctionInvocationFilter, ChatInterceptor>();

//  Register the Background Services
//  Scanners for Codebase and Documents/Knowledge
builder.Services.AddHostedService<EnrichmentWorker>();
builder.Services.AddHostedService<AgentTaskWorker>();

builder.Services.AddSingleton(Channel.CreateUnbounded<object>());

var kernelBuilder = builder.Services.AddKernel();

kernelBuilder.Plugins.AddFromType<CodebaseContextSearchTool>();
kernelBuilder.Plugins.AddFromType<DocumentContextSearchTool>();
kernelBuilder.Plugins.AddFromType<LocalFileSystemTool>();
kernelBuilder.Plugins.AddFromType<ReferenceContextSearchTool>();

using IHost host = builder.Build();
await host.StartAsync();

using (var scope = host.Services.CreateAsyncScope())
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

    logger.LogInformation("Ingestion complete. Start MCP Flight Deck ...");

    var config = services.GetRequiredService<IConfiguration>();

    //  TODO:   Feature:    Buildout MLOps workflows
    var enrichmentDispatcher = services.GetRequiredService<EnrichmentDispatcher>();
    await enrichmentDispatcher.RunAsync();
}

await host.StopAsync();
