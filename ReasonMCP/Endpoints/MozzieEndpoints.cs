using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ReasonMCP.DTOs;
using ReasonMCP.Orchestration;

namespace ReasonMCP.Endpoints
{
    public static class MozzieEndpoints
    {
        public static void MapMozzieFileScanEndpoints(
            this WebApplication app
        )
        {
            app.MapPost("/api/v1/mozzie", async (
                [FromBody] VSCodeChatPayloadDto payload,
                [FromServices] MozzieFileOrchestrator mozzie
            ) =>
            {

            });
        }
    }
}