using System.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Agents.Tools
{
    public class WriteFileTool
    {
        private readonly ILocalFileSystemService _localFileSystemService;
        private readonly string _agentId;
        private readonly IOptionsMonitor<FileSystemSecuritySettings> _settings;
        private readonly ILogger<WriteFileTool> _logger;

        public WriteFileTool(
            ILocalFileSystemService localFileSystemService,
            string agentId,
            IOptionsMonitor<FileSystemSecuritySettings> settings,
            ILogger<WriteFileTool> logger
        )
        {
            _localFileSystemService = localFileSystemService;
            _agentId = agentId;
            _settings = settings;
            _logger = logger;
        }

        [McpServerTool(Name = "write_file_tool")]
        [KernelFunction("write_file_tool")]
        [Description("Provides agents the ability to write files generated from context")]
        public async Task<string> WriteFileAsync(
            [Description("The absolute file path where the content should be written.")] string absolutePath,
            [Description("The complete string content to write into the file.")] string content,
            CancellationToken cancellationToken = default
        )
        {
            // The Tool simply acts as a trigger. The Service handles the security and I/O.
            return await _localFileSystemService.WriteFileAsync(
                _agentId,
                absolutePath,
                content,
                cancellationToken
            );
        }
    }
}