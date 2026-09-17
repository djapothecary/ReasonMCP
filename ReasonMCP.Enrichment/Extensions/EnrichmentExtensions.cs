using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Services;
using ReasonMCP.Enrichment.Interfaces;
using ReasonMCP.Enrichment.Processors;
using ReasonMCP.Enrichment.Services;
using ReasonMCP.Enrichment.Utilities;
using ReasonMCP.Enrichment.Workflows;

namespace ReasonMCP.Enrichment.Extensions
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
            builder.Services.AddTransient<IIngestionQueueService, DapperIngestionQueueService>();
            builder.Services.AddScoped<IDocumentsProcessor, DocumentsProcessor>();
            builder.Services.AddScoped<IDocumentScanService, DocumentScanService>();
            builder.Services.AddScoped<IIngestEnrichedRecordsService, IngestEnrichedRecordsService>();
            builder.Services.AddScoped<IReferenceDataScanService, ReferenceDataScanService>();
            builder.Services.AddScoped<IReferenceDataProcessor, ReferenceDataProcessor>();
            builder.Services.AddScoped<IVectorSearchResultService, VectorSearchResultServices>();

            return builder;
        }

        public static IHostApplicationBuilder AddEnrichmentUtilities(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<IMhtmlConverterUtility, MhtmlConverterUtility>();
            builder.Services.AddScoped<IFileConverterUtility, FileConverterUtility>();
            builder.Services.AddScoped<IChunkParsingUtility, ChunkParsingUtility>();
            builder.Services.AddScoped<IMetadataEnrichmentUtility, MetadataEnrichmentUtility>();

            return builder;
        }
    }
}