using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Handlers;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;
using ReasonMCP.Processors;
using ReasonMCP.Utilities;

namespace ReasonMCP.Extensions
{
    public static class ReasonServiceExtensions
    {
        public static IHostApplicationBuilder AddFileServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<LoggingDelegatingHandler>();
            builder.Services.AddScoped<PreProcessOrchestrator>();
            builder.Services.AddScoped<FileIngestOrchestrator>();
            builder.Services.AddScoped<IFileConverterUtility, FileConverterUtility>();
            builder.Services.AddScoped<IDocumentProcessor, AdrProcessor>();
            builder.Services.AddScoped<IDocumentProcessor, NewsletterProcessor>();

            return builder;
        }
    }
}