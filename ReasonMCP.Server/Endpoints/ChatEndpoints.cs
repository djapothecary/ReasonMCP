using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Orchestration;
using ReasonMCP.Core.Utilities;

namespace ReasonMCP.Server.Endpoints
{
    public static class ChatEndpoints
    {
        public static void MapReasonChatEndpoints(
            this WebApplication app)
        {
            app.MapPost("/api/v1/chat", async (
                [FromBody] VSCodeChatPayloadDto payload,
                [FromServices] SemanticKernelWrapperOrchestrator orchestrator
            ) =>
            {
                //  Log the received prompt and history
                Console.WriteLine($"\n[VS CODE INTERCEPT] Received prompt: {payload.Prompt}");
                Console.WriteLine($"[VS CODE INTERCEPT] History items: {payload.History.Count}");

                //  Create Cancellation Token
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var cancellationToken = cts.Token;

                //  Send the message off for processing
                var response = await orchestrator.ProcessChatAsync(payload);

                return await EndpointResponseUtility.CheckResponseStringErrorsAsync(
                    payload,
                    response,
                    cancellationToken
                );
            });
        }
    }
}