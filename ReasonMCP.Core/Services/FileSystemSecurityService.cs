using Microsoft.Extensions.Options;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;

namespace ReasonMCP.Core.Services
{
    public class FileSystemSecurityService : IFileSystemSecurityService
    {
        private readonly IAgentProfileService _agentProfileService;
        private readonly IOptionsMonitor<FileSystemSecuritySettings> _settings;
        private readonly IOptionsMonitor<ChatSettings> _agentChatSettings;

        public FileSystemSecurityService(
            IAgentProfileService agentProfileService,
            IOptionsMonitor<FileSystemSecuritySettings> settings,
            IOptionsMonitor<ChatSettings> agentChatSettings
        )
        {
            _agentProfileService = agentProfileService;
            _settings = settings;
            _agentChatSettings = agentChatSettings;
        }

        public bool IsPathAllowed(string requestedPath)
        {
            var allowedRoots = _settings.CurrentValue.AllowedRootDirectories;

            //  1.  Resolve any "sneaky" directories (e.g. C:\Source\...\Windows\System32)
            string fullPath = Path.GetFullPath(requestedPath);

            //  2.  Ensure it starts with one of the allowed roots
            return allowedRoots.Any(
                root => fullPath.StartsWith(
                    root,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        public async Task<bool> IsAgentAllowedForWriteHereAsync(
            string agentId,
            string absolutePath
        )
        {
            //  1.  Get the Agent Profile
            var agentProfile = await _agentProfileService.LoadAgentProfileAsync(
                    _agentChatSettings.CurrentValue.Agents[agentId].AgentProfilePath
                );

            //  2. Get the Allowed Workind Directories from the Agent Profile
            var allowedWorkingDirectories = agentProfile
                .Permissions
                .AllowedWorkingDirectories;

            //  3.  Get the full system path incase the agent tries to be sneaky
            // Check the path against allowed directories with out using any concatonation
            var fullPath = Path.GetFullPath(absolutePath);

            return allowedWorkingDirectories.Any(
                workingDirs => fullPath.StartsWith(
                    workingDirs,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }
    }
}