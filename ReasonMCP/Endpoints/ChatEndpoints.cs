using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using ReasonMCP.DTOs;


namespace ReasonMCP.Endpoints
{
    public static class ChatEndpoints
    {
        public static void MapReasonChatEndpoints(this WebApplication app)
        {
            app.MapPost("/api/v1/chat", async (
                [FromBody] ChatRequest request
            ) =>
            {
                //  Log the received prompt and history
                Console.WriteLine($"\n[VS CODE INTERCEPT] Received prompt: {request.Prompt}");
                Console.WriteLine($"[VS CODE INTERCEPT] History items: {request.History.Count}");

                //  Process the history
                foreach (var message in request.History)
                {
                    Console.WriteLine($"  [{message.Role}]: {message.Content.Substring(0, Math.Min(100, message.Content.Length))}...");
                }

                return Results.Ok(new
                {
                    response = $"**Ping-Pong Successful!** \n\nReason Backend received the prompt: *\"{request.Prompt}\"*\n\nHistory contains {request.History.Count} previous messages."
                });
            });
        }
    }
}