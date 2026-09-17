using Microsoft.AspNetCore.Routing;
using ReasonMCP.Server.Endpoints;

namespace ReasonMCP.Server.Extensions
{
    public static class EndpointsExtension
    {
        public static void MapHealthEndpoints(
            this IEndpointRouteBuilder app
        )
        {
            HealthEndpoints.MapHealthEndpoints(app);
        }
    }
}