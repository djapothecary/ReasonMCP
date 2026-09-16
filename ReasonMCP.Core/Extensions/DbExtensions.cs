using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Data;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Extensions
{
    public static class DbExtensions
    {
        public static IHostApplicationBuilder AddDbInitializers(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<CodebaseVectorDbInitializer>();
            builder.Services.AddTransient<DocumentsVectorDbInitializer>();
            builder.Services.AddTransient<ReferenceVectorDbInitializer>();

            return builder;
        }

        public static IHostApplicationBuilder AddDbFactories(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddSingleton<ICodebaseDbConnectionFactory, CodebaseDbConnectionFactory>();
            builder.Services.AddSingleton<IDocumentsDbConnectionFactory, DocumentsDbConnectionFactory>();
            builder.Services.AddSingleton<IReferenceDbConnectionFactory, ReferenceDbConnectionFactory>();

            return builder;
        }
    }
}