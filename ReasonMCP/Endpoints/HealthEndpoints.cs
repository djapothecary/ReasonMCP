using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace ReasonMCP.Endpoints
{
    public class HealthEndpoints
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/health");

            group.MapGet("/", async (
            ILogger<HealthEndpoints> logger) =>
            {
                try
                {
                    return Results.Ok(new
                    {
                        Server = "ReasonMCP Neural Terminal",
                        Status = "Online",
                        Transport = "HTTP/SSE",
                        SseEndpoint = "http://localhost:5000/sse",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.ToString(), statusCode: 500);
                }
            });
        }
    }
}