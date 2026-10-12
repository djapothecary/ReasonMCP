using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReasonMCP.Skills.Interfaces;
using ReasonMCP.Skills.Services;
using ReasonMCP.Skills.Tools;

namespace ReasonMCP.Skills.Extensions
{
    public static class SkillsExtensions
    {
        public static IHostApplicationBuilder AddSkillsServices(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<ISkillLibraryService, LocalSkillLibraryService>();

            return builder;
        }
        public static IHostApplicationBuilder AddSkillsTools(
            this IHostApplicationBuilder builder
        )
        {
            builder.Services.AddScoped<MicrosoftSkillsTools>();

            return builder;
        }
    }
}