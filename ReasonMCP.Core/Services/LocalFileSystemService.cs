using System.Text;
using Microsoft.Extensions.Logging;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Core.Services
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

        public async Task<List<FileAttachmentRecord>> GenerateDirectoryListAsync(
            string agentId,
            string absolutePath,
            CancellationToken cancellationToken
        )
        {
            var directoryFileAttachementRecords = new List<FileAttachmentRecord>();
            //  1.  The Guardrail (never trust the LLM)
            if (!_securityService.IsPathAllowed(absolutePath))
            {
                directoryFileAttachementRecords.Add(
                    new FileAttachmentRecord(
                        string.Empty,
                        string.Empty,
                        absolutePath,
                        0,
                        $"[SYSTEM DIRECTIVE]: Access to '{absolutePath}' is DENIED by enterprise policy. Stop and ask Apoth for a valid path.",
                        true
                    )
                );

                _logger.LogWarning("[SECURITY ALERT]:  {Agent} attempted to read restricted path: {Path}", agentId, absolutePath);
                return directoryFileAttachementRecords;
            }

            if (!Directory.Exists(absolutePath))
            {
                directoryFileAttachementRecords.Add(
                    new FileAttachmentRecord(
                        string.Empty,
                        string.Empty,
                        absolutePath,
                        0,
                        $"Directory not found: {absolutePath}",
                        true
                    )
                );
                return directoryFileAttachementRecords;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"# Contents of {absolutePath}");

                //  Directories
                foreach (var dir in Directory.GetDirectories(absolutePath))
                {
                    var dirInfo = new DirectoryInfo(dir);
                    directoryFileAttachementRecords.Add(
                        new FileAttachmentRecord(
                            string.Empty,
                            dirInfo.Name,
                            absolutePath,
                            0,
                            $"- 📁 **{dirInfo.Name}/** (Directory)",
                            false
                        )
                    );
                }

                //  Files
                foreach (var file in Directory.GetFiles(absolutePath))
                {
                    var fileInfo = new FileInfo(file);
                    //  Convert bytes to KB for context
                    long sizeKb = fileInfo.Length / 1024;
                    directoryFileAttachementRecords.Add(
                        new FileAttachmentRecord(
                            fileInfo.Name,
                            fileInfo.FullName,
                            absolutePath,
                            sizeKb,
                            $"- 📄 {fileInfo.Name} ({sizeKb} KB)",
                            false
                        )
                    );
                }

                return directoryFileAttachementRecords;
            }
            catch (UnauthorizedAccessException)
            {
                directoryFileAttachementRecords.Add(
                    new FileAttachmentRecord(
                        string.Empty,
                        string.Empty,
                        absolutePath,
                        0,
                        $"[ERROR]: Permission denied when trying to read {absolutePath}.",
                        true
                    )
                );

                return directoryFileAttachementRecords;
            }
        }

        public async Task<string> WriteFileAsync(
            string agentId,
            string absolutePath,
            string content,
            CancellationToken cancellationToken
        )
        {
            //  1.  Check Global System Lock AND Agent Specific sandboxes
            // 1. Check Global System Lock (Sync)
            if (!_securityService.IsPathAllowed(absolutePath))
            {
                return $"[SYSTEM DIRECTIVE]: DENIED. '{absolutePath}' violates global policy.";
            }

            // 2. Check Agent Sandbox (Async!)
            if (!await _securityService.IsAgentAllowedForWriteHereAsync(agentId, absolutePath))
            {
                return $"[SYSTEM DIRECTIVE]: DENIED. You ({agentId}) do not have sandbox permissions for this directory.";
            }

            try
            {
                //  Ensure that the directory exists before writing
                var directory = Path.GetDirectoryName(absolutePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //  2.  Write the file asynchronously
                await File.WriteAllTextAsync(
                    absolutePath,
                    content,
                    cancellationToken
                );

                _logger.LogInformation("[FILE SYSTEM]: {Agent} successfully wrote to {Path}", agentId, absolutePath);
                return $"[SUCCESS]: File written successfully to {absolutePath}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FILE SYSTEM ERROR]: Failed to write file {Path}", absolutePath);
                return $"[ERROR]: An unexpected OS error occurred while writing the file: {ex.Message}";
            }
        }
    }
}