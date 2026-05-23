using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using ReasonMCP.DTOs;


namespace ReasonMCP.Endpoints
{
    public static class AiGatewayFallbackEndpoints
    {
        public static void MapAiGatewayEndpoints(this WebApplication app)
        {
            // This is the front door that VS Code will hit instead of Ollama
            // app.MapPost("/api/v1/chat", async (
            app.MapPost("/api/v1", async (
                [FromBody] VSCodeChatPayloadDto incomingPayload,
                [FromServices] Kernel kernel,
                HttpContext context) =>
            {
                // 1. The payload hits C#.
                // 2. We pass the messages to Semantic Kernel.
                // 3. Our ReasonChatInterceptor (IFunctionInvocationFilter) FIRES HERE!
                // 4. We return the LLM's response back to VS Code.

                return Results.Ok(new { response = "MITM Intercept Successful." });
            });
        }

        public static void MapAiTestInterceptEndpoints(this WebApplication app)
        {
            // 1. The Fake Ollama Heartbeat
            app.MapGet("/api/version", () =>
            {
                Console.WriteLine("[HONEYPOT] Answered Ollama Version Ping.");
                return Results.Ok(new { version = "0.1.34" }); // Standard Ollama version format
            });

            // 2. The Fake Ollama Tag List (Sometimes clients ping this to see what models you have)
            app.MapGet("/api/tags", () =>
            {
                Console.WriteLine("[HONEYPOT] Answered Ollama Model List Ping.");
                return Results.Ok(new
                {
                    models = new[] {
            new { name = "Reason:latest", model = "Reason:latest", details = new { family = "llama" } }
                    }
                });
            });

            // 3. The Catch-All for the actual POST request
            app.MapFallback(async (HttpContext context) =>
            {
                var request = context.Request;
                Console.WriteLine($"\n[HONEYPOT TRIGGERED] {request.Method} {request.Path}");

                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                Console.WriteLine($"[PAYLOAD]: {body}\n");

                // Return the fake response
                return Results.Ok(new
                {
                    id = "chatcmpl-123",
                    choices = new[] { new { message = new { content = "I am the honeypot." } } }
                });
            });

            app.MapPost("v1/traces", async (HttpContext context) =>
            {
                Console.WriteLine("\n[oTEL INTERCEPTED] Received Copilot Telemetry!");

                // Check the content type to see if VS Code is sending JSON or Protobuf
                var contentType = context.Request.ContentType;
                Console.WriteLine($"[Content-Type]: {contentType}");

                using var memoryStream = new MemoryStream();
                await context.Request.Body.CopyToAsync(memoryStream);
                var bodyBytes = memoryStream.ToArray();

                if (contentType != null && contentType.Contains("json"))
                {
                    // If it's JSON, we can read it directly!
                    var json = System.Text.Encoding.UTF8.GetString(bodyBytes);
                    Console.WriteLine($"[JSON PAYLOAD]: {json}");
                }
                else
                {
                    // If it's application/x-protobuf, it's binary.
                    Console.WriteLine($"[BINARY PAYLOAD]: Received {bodyBytes.Length} bytes of Protobuf data.");
                }

                // oTel requires a 200 OK to know the trace was received successfully
                return Results.Ok();
            });
        }
    }

}