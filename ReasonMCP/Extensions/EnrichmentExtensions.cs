using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Interfaces.IEnrichment;
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
            builder.Services.AddScoped<IDocumentScanService, DocumentScanService>();
            builder.Services.AddScoped<IReferenceDataScanService, ReferenceDataScanService>();

            return builder;
        }
    }
}