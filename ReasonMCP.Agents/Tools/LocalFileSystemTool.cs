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
    public class LocalFileSystemTool
    {
        private readonly ILocalFileSystemService _localFileSystemService;
        private readonly string _agentId;
        private readonly FileSystemSecuritySettings _securitySettings;

        public LocalFileSystemTool(
            ILocalFileSystemService localFileSystemService,
            string agentId,
            FileSystemSecuritySettings securitySettings
        )
        {
            _localFileSystemService = localFileSystemService;
            _agentId = agentId;
            _securitySettings = securitySettings;
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
            if (!_securitySettings.Enabled)
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