using Microsoft.AspNetCore.Routing;
using ReasonMCP.Endpoints;

namespace ReasonMCP.Extensions
{
    public static class ReasonEndpointsExtension
    {
        public static void MapHealthEndpoints(
            this IEndpointRouteBuilder app
        )
        {
            HealthEndpoints.MapEndpoints(app);
        }
    }
}