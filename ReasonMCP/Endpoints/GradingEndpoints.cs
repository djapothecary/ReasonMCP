using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ReasonMCP.DTOs;
using ReasonMCP.Orchestration;

namespace ReasonMCP.Endpoints
{
    public static class GradingEndpoints
    {
        public static void MapGradingEndpoints(
            this WebApplication app)
        {
            app.MapPost("/api/v1/grading", async (
            ) =>
            {
                //  TODO:   Feature:    This endpoint will be used to perform grading operations
            });
        }
    }
}