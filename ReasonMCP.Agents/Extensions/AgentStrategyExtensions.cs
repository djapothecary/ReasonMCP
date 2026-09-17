using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Enrichment.Interfaces;
using ReasonMCP.Enrichment.Strategies.Converters;

namespace ReasonMCP.Agents.Extensions
{
    public static class AgentStrategyExtensions
    {
        public static IHostApplicationBuilder AddStrategies(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<IFileConverterStrategy, ConfigConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, MarkdownFileStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, MarkupConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, MhtmlConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, PdfConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, SourceCodeConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, SqlScriptConverterStrategy>();
            builder.Services.AddScoped<IFileConverterStrategy, TxtConverterStrategy>();

            return builder;
        }
    }
}