using System.ComponentModel;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;
using ReasonMCP.Models;

namespace ReasonMCP.Tools
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
        public async Task<string> ListDirectoryAsync(
            [Description("""
                The absolute path of the directory to inspect (e.g. 'C:\\Source\\ReasonData')
                """)] string absolutePath,
            CancellationToken cancellationToken
        )
        {
            if (!_settings.CurrentValue.Enabled)
                return "Local File System Scanning is not Enabled!";

            var scannedFilePath = await _localFileSystemService.GenerateDirectoryListAsync(
                absolutePath,
                cancellationToken
            );

            return scannedFilePath ?? string.Empty;
        }

    }
}