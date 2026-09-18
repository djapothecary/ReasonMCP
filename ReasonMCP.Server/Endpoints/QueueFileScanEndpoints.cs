using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Orchestration;

namespace ReasonMCP.Server.Endpoints
{
    public static class QueueFileScanEndpoints
    {
        public static void MapQueueFileScanEndpoints(
            this WebApplication app
        )
        {
            app.MapPost("/api/v1/workspace/queue/scan", async (
                [FromBody] VSCodeChatPayloadDto payload,
                [FromServices] MozzieFileOrchestrator mozzie
            ) =>
            {

            });
        }
    }
}