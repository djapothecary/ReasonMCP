using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Data;
using ReasonMCP.Core.Handlers;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Services;
using ReasonMCP.Core.Utilities;

namespace ReasonMCP.Core.Extensions
{
    public static class ServiceExtensions
    {
        public static IHostApplicationBuilder AddSessionServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddTransient<LoggingDelegatingHandler>();
            builder.Services.AddScoped<SessionContextManager>();

            return builder;
        }
    }
}