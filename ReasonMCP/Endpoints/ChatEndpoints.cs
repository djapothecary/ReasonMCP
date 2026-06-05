using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using ReasonMCP.DTOs;
using ReasonMCP.Orchestration;


namespace ReasonMCP.Endpoints
{
    public static class ChatEndpoints
    {
        public static void MapReasonChatEndpoints(this WebApplication app)
        {
            app.MapPost("/api/v1/chat", async (
                [FromBody] VSCodeChatPayloadDto payload,
                [FromServices] SemanticKernelWrapperOrchestrator orchestrator
            ) =>
            {
                //  Log the received prompt and history
                Console.WriteLine($"\n[VS CODE INTERCEPT] Received prompt: {payload.Prompt}");
                Console.WriteLine($"[VS CODE INTERCEPT] History items: {payload.History.Count}");

                //  Process the history
                foreach (var message in payload.History)
                {
                    Console.WriteLine($"  [{message.Role}]: {message.Content.Substring(0, Math.Min(100, message.Content.Length))}...");
                }

                //  Send the message off for processing
                var response = await orchestrator.ProcessChatAsync(payload, "chat");

                return Results.Ok(new
                {
                    // response = $"**Ping-Pong Successful!** \n\nReason Backend received the prompt: *\"{payload.Prompt}\"*\n\nHistory contains {payload.History.Count} previous messages."
                    response
                });
            });
        }
    }
}