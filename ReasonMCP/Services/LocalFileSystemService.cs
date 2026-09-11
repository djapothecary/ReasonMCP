using System.Text;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Services
{
    public class LocalFileSystemService : ILocalFileSystemService
    {
        private readonly IFileSystemSecurityService _securityService;
        private readonly ILogger<LocalFileSystemService> _logger;

        public LocalFileSystemService(
            IFileSystemSecurityService securityService,
            ILogger<LocalFileSystemService> logger
        )
        {
            _securityService = securityService;
            _logger = logger;
        }

        public async Task<string> GenerateDirectoryListAsync(
            string absolutePath,
            CancellationToken cancellationToken
        )
        {
            //  1.  The Guardrail (never trust the LLM)
            if (!_securityService.IsPathAllowed(absolutePath))
            {
                _logger.LogWarning("[SECURITY ALERT]:  Mozzie attempted to read restricted path: {Path}", absolutePath);
                return $"[SYSTEM DIRECTIVE]: Access to '{absolutePath}' is DENIED by enterprise policy. Stop and ask Apoth for a valid path.";
            }

            if (!Directory.Exists(absolutePath))
            {
                return $"Directory not found: {absolutePath}";
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"# Contents of {absolutePath}");

                //  Directories
                foreach (var dir in Directory.GetDirectories(absolutePath))
                {
                    var dirInfo = new DirectoryInfo(dir);
                    sb.AppendLine($"- 📁 **{dirInfo.Name}/** (Directory)");
                }

                //  Files
                foreach (var file in Directory.GetFiles(absolutePath))
                {
                    var fileInfo = new FileInfo(file);
                    //  Convert bytes to KB for context
                    long sizeKb = fileInfo.Length / 1024;
                    sb.AppendLine($"- 📄 {fileInfo.Name} ({sizeKb} KB)");
                }

                return sb.ToString();
            }
            catch (UnauthorizedAccessException)
            {
                return $"[ERROR]: Permission denied when trying to read {absolutePath}.";
            }
        }

    }
}