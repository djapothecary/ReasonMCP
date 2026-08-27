using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;
using ReasonMCP.Processors;
using ReasonMCP.Services;
using ReasonMCP.Services.Enrichment;
using ReasonMCP.Workflows;

namespace ReasonMCP.Extensions
{
    public static class EnrichmentExtensions
    {
        public static IHostApplicationBuilder AddWorkflows(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<CodebaseWorkflow>();
            builder.Services.AddTransient<DocumentWorkflow>();
            builder.Services.AddTransient<ReferenceWorkflow>();

            return builder;
        }

        public static IHostApplicationBuilder AddEnrichmentServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<ICodebaseScanService, CodebaseScanService>();
            builder.Services.AddScoped<ICodebaseProcessor, CodebaseProcessor>();
            builder.Services.AddScoped<ICodebaseRecordIngestionService, CodebaseRecordIngestionService>();
            builder.Services.AddScoped<IDocumentsProcessor, DocumentsProcessor>();
            builder.Services.AddScoped<IDocumentScanService, DocumentScanService>();
            builder.Services.AddScoped<IIngestEnrichedRecordsService, IngestEnrichedRecordsService>();
            builder.Services.AddScoped<IReferenceDataScanService, ReferenceDataScanService>();
            builder.Services.AddScoped<IReferenceDataProcessor, ReferenceDataProcessor>();
            builder.Services.AddScoped<IVectorSearchResultService, VectorSearchResultServices>();

            return builder;
        }
    }
}