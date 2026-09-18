using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Services;
using ReasonMCP.Enrichment.Dispatchers;
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
            builder.Services.AddSingleton<ICodebaseScanService, CodebaseScanService>();
            builder.Services.AddSingleton<ICodebaseProcessor, CodebaseProcessor>();
            builder.Services.AddScoped<ICodebaseRecordIngestionService, CodebaseRecordIngestionService>();
            builder.Services.AddTransient<IIngestionQueueService, DapperIngestionQueueService>();
            builder.Services.AddSingleton<IDocumentsProcessor, DocumentsProcessor>();
            builder.Services.AddSingleton<IDocumentScanService, DocumentScanService>();
            builder.Services.AddSingleton<IIngestEnrichedRecordsService, IngestEnrichedRecordsService>();
            builder.Services.AddSingleton<IReferenceDataScanService, ReferenceDataScanService>();
            builder.Services.AddSingleton<IReferenceDataProcessor, ReferenceDataProcessor>();
            builder.Services.AddSingleton<IVectorSearchResultService, VectorSearchResultServices>();

            return builder;
        }

        public static IHostApplicationBuilder AddEnrichmentUtilities(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<IMhtmlConverterUtility, MhtmlConverterUtility>();
            builder.Services.AddSingleton<IFileConverterUtility, FileConverterUtility>();
            builder.Services.AddSingleton<IChunkParsingUtility, ChunkParsingUtility>();
            builder.Services.AddSingleton<IMetadataEnrichmentUtility, MetadataEnrichmentUtility>();

            return builder;
        }

        public static IHostApplicationBuilder AddEnrichmentDispatcher(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<EnrichmentDispatcher>();

            return builder;
        }
    }
}