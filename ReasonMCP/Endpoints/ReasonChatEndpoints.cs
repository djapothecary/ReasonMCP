using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using ReasonMCP.DTOs;


namespace ReasonMCP.Endpoints
{
    public static class ReasonChatEndpoints
    {
        public static void MapReasonChatEndpoints(this WebApplication app)
        {
            app.MapPost("/api/v1/chat", async (
                [FromBody] ReasonChatRequest request
            ) =>
            {
                //  The Ping-Pong test:  Just echo back what was received
                Console.WriteLine($"\n[VS CODE INTERCEPT] Received prompt: {request.Prompt}");

                return Results.Ok(new
                {
                    response = $"**Ping-Pong Successfull!** \n\nReason Backend received the prompt: *\"{request.Prompt}\"*"
                });
            });
        }
    }
}