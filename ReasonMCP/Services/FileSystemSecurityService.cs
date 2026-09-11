using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Services
{
    public class FileSystemSecurityService : IFileSystemSecurityService
    {
        private readonly IOptionsMonitor<FileSystemSecuritySettings> _settings;

        public FileSystemSecurityService(
            IOptionsMonitor<FileSystemSecuritySettings> settings
        )
        {
            _settings = settings;
        }

        public bool IsPathAllowed(string requestedPath)
        {
            var allowedRoots = _settings.CurrentValue.AllowedRootDirectories;

            //  1.  Resolve any "sneaky" directories (e.g. C:\Source\...\Windows\System32)
            string fullPath = Path.GetFullPath(requestedPath);

            //  2.  Ensure it starts with one of the allowed roots
            return allowedRoots.Any(
                root => fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}