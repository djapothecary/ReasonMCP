using Microsoft.AspNetCore.Routing;
using ReasonMCP.Endpoints;

namespace ReasonMCP.Extensions
{
    public static class EndpointsExtension
    {
        public static void MapHealthEndpoints(
            this IEndpointRouteBuilder app
        )
        {
            HealthEndpoints.MapEndpoints(app);
        }
    }
}