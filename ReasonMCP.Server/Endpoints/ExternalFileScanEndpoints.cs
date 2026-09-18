using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Orchestration;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Server.Endpoints
{
    public static class ExternalFileScanEndpoints
    {
        public static void MapExternalFileScanEndpoints(
            this WebApplication app
        )
        {
            app.MapPost("/api/v1/external/scan", async (
                [FromBody] VSCodeChatPayloadDto payload,
                [FromServices] IServiceScopeFactory scopeFactory
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
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                var cancellationToken = cts.Token;

                //  Build scope and get services
                using var scope = scopeFactory.CreateScope();
                var localFileSystemScanService = scope
                    .ServiceProvider
                    .GetRequiredService<ILocalFileSystemService>();

                //  combine attached files
                var combinedFileAttachements = new List<FileAttachmentDto>();
                // 2. Elegantly combine both lists using LINQ Concat
                var combinedFileAttachments = payload.Attachments
                    .Concat(payload.ExternallyAttachedFiles)
                    .ToList();

                var localFilesScanResponse = new List<FileAttachmentRecord>();

                // 3. Process sequentially and flatten the result
                foreach (var attachment in combinedFileAttachments)
                {
                    var responses = await localFileSystemScanService
                        .GenerateDirectoryListAsync(
                            payload.AgentId,
                            attachment.FileName,
                            cancellationToken
                        );

                    // FLATTENED: AddRange eliminates the need for the nested foreach loop
                    localFilesScanResponse.AddRange(responses);
                }

                return Results.Ok(new
                {

                });
            });
        }
    }
}