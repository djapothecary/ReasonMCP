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
    public class ReadTestResultsTool
    {
        private readonly ILocalFileSystemService _localFileSystemService;
        private readonly string _agentId;
        private readonly FileSystemSecuritySettings _securitySettings;

        public ReadTestResultsTool(
            ILocalFileSystemService localFileSystemService,
            string agentId,
            FileSystemSecuritySettings securitySettings
        )
        {
            _localFileSystemService = localFileSystemService;
            _agentId = agentId;
            _securitySettings = securitySettings;
        }
    }
}