using System.ComponentModel;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Models;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Agents.Tools
{
    public class LocalFileSystemTool
    {
        private readonly ILocalFileSystemService _localFileSystemService;
        private readonly IOptionsMonitor<FileSystemSecuritySettings> _settings;
        private readonly ILogger<LocalFileSystemTool> _logger;

        public LocalFileSystemTool(
            ILocalFileSystemService localFileSystemService,
            IOptionsMonitor<FileSystemSecuritySettings> settings,
            ILogger<LocalFileSystemTool> logger
        )
        {
            _localFileSystemService = localFileSystemService;
            _settings = settings;
            _logger = logger;
        }

        [McpServerTool(Name = "local_filesystem_tool")]
        [KernelFunction("local_filesystem_tool")]
        [Description("Lists all files and subdirectories within a given absolute directory path.")]
        public async Task<List<FileAttachmentRecord>> ListDirectoryAsync(
            [Description("""
                The absolute path of the directory to inspect (e.g. 'C:\\Source\\ReasonData')
                """)] string absolutePath,
            [Description("""
                The ID of the Agent making the request
            """)] string agentId,
            CancellationToken cancellationToken
        )
        {
            var localFileSystemToolResponse = new List<FileAttachmentRecord>();
            if (!_settings.CurrentValue.Enabled)
            {
                var notEnabledResponse = new FileAttachmentRecord(
                        string.Empty,
                        string.Empty,
                        absolutePath,
                        0,
                        "Local File System Scanning is not Enabled!",
                        true
                    );

                localFileSystemToolResponse.Add(notEnabledResponse);
                return localFileSystemToolResponse;
            }

            var scannedFilePaths = await _localFileSystemService
                .GenerateDirectoryListAsync(
                    agentId,
                    absolutePath,
                    cancellationToken
                );

            foreach (var file in scannedFilePaths)
            {
                localFileSystemToolResponse.Add(file);
            }

            return localFileSystemToolResponse;
        }

    }
}