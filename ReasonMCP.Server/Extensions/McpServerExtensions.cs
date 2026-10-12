using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ReasonMCP.Agents.Tools;
using ReasonMCP.Skills.Tools;

namespace ReasonMCP.Server.Extensions
{
    public static class McpServerExtensions
    {
        public static WebApplicationBuilder AddReasonMcpServer(
            this WebApplicationBuilder builder
        )
        {
            // Add the MCP services: the transport to use and the tools to register.
            builder.Services
                .AddMcpServer()
                .WithHttpTransport()
                .WithTools<RandomNumberTools>()
                .WithTools<CodebaseContextSearchTool>()
                .WithTools<DocumentContextSearchTool>()
                .WithTools<LocalFileSystemTool>()
                .WithTools<MicrosoftSkillsTools>()
                .WithTools<ReadTestResultsTool>()
                .WithTools<ReferenceContextSearchTool>()
                .WithTools<RunUnitTestsTool>()
                .WithTools<WriteFileTool>();

            return builder;
        }
    }
}