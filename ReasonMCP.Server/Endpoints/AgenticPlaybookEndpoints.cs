using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Orchestration;
using ReasonMCP.Core.Utilities;
using ReasonMCP.Core.Workflows;

namespace ReasonMCP.Server.Endpoints
{
    public static class AgenticPlaybookEndpoints
    {
        public static void MapAgenticPlaybookEndpoints(
            this WebApplication app
        )
        {
            app.MapPost("/api/v1/playbook/load", async (
                [FromBody] VSCodeChatPayloadDto payload,
                [FromServices] IServiceScopeFactory scopeFactory,
                [FromServices] IOptions<PlaybookSettings> settings
            ) =>
            {
                //  Bail out if there are no files attached
                if (payload.Attachments == null
                    || payload.ExternallyAttachedFiles == null)
                {
                    var badRequestResponse = "No files were attached.";
                    return Results.BadRequest(new
                    {
                        badRequestResponse
                    });
                }

                //  Create Cancellation Token
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var cancellationToken = cts.Token;

                //  Build scope and get services
                using var scope = scopeFactory.CreateScope();
                var playbookWorkflow = scope
                    .ServiceProvider
                    .GetRequiredService<IPlaybookWorkflow>();

                // build playbook path
                var filePath = payload.Attachments[0].FilePath;

                var playbookResponse = await playbookWorkflow.ProcessPlaybookWorkflowAsync(
                    filePath,
                    payload.SessionId,
                    payload.Prompt.TrimEnd(),
                    cancellationToken
                );

                var playbookChatHistoryErrorCheck = new ChatHistory();
                playbookChatHistoryErrorCheck.AddAssistantMessage(playbookResponse);

                return await EndpointResponseUtility.CheckResponseErrorsAsync(
                    payload,
                    playbookChatHistoryErrorCheck,
                    cancellationToken
                );
            });

            _ = app.MapPost("/api/v1/playbook/create", async (
                [FromBody] VSCodeChatPayloadDto payload,
                [FromServices] IServiceScopeFactory scopeFactory,
                [FromServices] IOptions<PlaybookSettings> settings
            ) =>
            {
                //  Bail out if there are no files attached
                if (payload.Attachments == null
                    || payload.ExternallyAttachedFiles == null)
                {
                    var badRequestResponse = "No files were attached.";
                    return Results.BadRequest(new
                    {
                        badRequestResponse
                    });
                }

                //  Create Cancellation Token
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var cancellationToken = cts.Token;

                //  Build scope and get services
                using var scope = scopeFactory.CreateScope();
                //  get GLaDOS
                var gladosAgent = scope
                    .ServiceProvider
                    .GetRequiredService<IGladosAgent>();

                var gladosResponse = await gladosAgent.CreatePlaybookAsync(
                    payload,
                    cancellationToken
                );

                return await EndpointResponseUtility.CheckResponseErrorsAsync(
                    payload,
                    gladosResponse,
                    cancellationToken
                );
            });
        }
    }
}