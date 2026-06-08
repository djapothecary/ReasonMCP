using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ReasonMCP.DTOs;
using ReasonMCP.Orchestration;

namespace ReasonMCP.Endpoints
{
    public static class TroubleshootingEndpoints
    {
        public static void MapTroubleshootingEndpoints(this WebApplication app)
        {
            app.MapPost("/api/v1/chat", async (
                [FromBody] VSCodeChatPayloadDto request,
                HttpContext httpContext) =>
            {
                try
                {
                    // Force the DI container to build the orchestrator manually
                    var orchestrator = httpContext.RequestServices.GetRequiredService<SemanticKernelWrapperOrchestrator>();

                    Console.WriteLine("\n[SUCCESS] Orchestrator built perfectly!");
                    return Results.Ok(new { response = "DI resolved successfully." });
                }
                catch (Exception ex)
                {
                    // THIS WILL TELL YOU EXACTLY WHAT IS BROKEN
                    Console.WriteLine($"\n[DI FATAL ERROR]: {ex.Message}");

                    return Results.Problem(detail: ex.Message, statusCode: 500);
                }
            });
        }

        public static void MapRawAPIEndpoint(this WebApplication app)
        {
            //  For testing payload configurations
            app.MapPost("/api/v1/chat", async (HttpContext context) =>
            {
                // PUT YOUR BREAKPOINT ON THE CONSOLE.WRITELINE BELOW
                Console.WriteLine("\n[GATEWAY] Request made it through the door!");

                // Read the raw body as a string to see exactly what TS sent
                using var reader = new StreamReader(context.Request.Body);
                var rawJson = await reader.ReadToEndAsync();

                Console.WriteLine($"[RAW JSON]: {rawJson}");

                return Results.Ok(new { response = "Raw connection successful." });
            });
        }
    }
}