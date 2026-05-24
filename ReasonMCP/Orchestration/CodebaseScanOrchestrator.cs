using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Services;

namespace ReasonMCP.Orchestration
{
    public class CodebaseScanOrchestrator
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<CodebaseScanOrchestrator> _logger;

        public CodebaseScanOrchestrator(
            IServiceScopeFactory scopeFactory,
            IOptions<CodebaseScanSettings> options,
            ILogger<CodebaseScanOrchestrator> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task ScanCodebaseAsync(
            CancellationToken cancellationToken = default
        )
        {
            var rootDirectory = _settings.RootDirectory;
            var rootDirInfo = new DirectoryInfo(rootDirectory);
            IEnumerable<DirectoryInfo> directoriesToScan;

            if (_settings.SubDirectories.Count > 0)
            {
                var dirList = new List<DirectoryInfo>();
                foreach (var subDir in _settings.SubDirectories)
                {
                    //  Intentionally not using Path.Combine
                    //  Since "subDir" is an absolute path, only subDir is returned by Path.Combine
                    var fullPath = rootDirectory + subDir;
                    if (Directory.Exists(fullPath))
                    {
                        dirList.Add(new DirectoryInfo(fullPath));
                    }
                }
                directoriesToScan = dirList;
            }
            else
            {
                directoriesToScan = rootDirInfo.EnumerateDirectories("*", SearchOption.AllDirectories);
            }

            foreach (var dir in directoriesToScan)
            {
                _logger.LogTrace($"Processing Code directory: {dir.FullName}");
                await ProcessDirectoryRecursivelyAsync(dir, cancellationToken);
            }
        }

        /// <summary>
        /// Recursively processes a directory and its subdirectories.
        /// Handles file processing for leaf directories and continues traversal for directories with subdirectories.
        /// </summary>
        /// <param name="directory">The directory to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task ProcessDirectoryRecursivelyAsync(
            DirectoryInfo directory,
            CancellationToken cancellationToken = default)
        {
            var subDirectories = GetIncludedFilteredDirectoriesList(directory);

            if (subDirectories == null || !subDirectories.Any())
                return;

            var scope = _scopeFactory.CreateScope();
            var dapperIngestionQueue = scope.ServiceProvider.GetRequiredService<DapperIngestionQueueService>();

            foreach (var subdirectory in subDirectories)
            {
                _logger.LogTrace($"Processing Codebase Project directory: {subdirectory.FullName}");

                var files = GetIncludedFilteredFilesList(subdirectory);
                if (files.Count > 0)
                {
                    //  Handle any "straggler files" for processing here
                    foreach (var file in files)
                    {
                        await dapperIngestionQueue.UpsertToQueueAsync(file.FullName, "Codebase", file.LastWriteTimeUtc, cancellationToken);
                        Console.WriteLine(file.FullName);
                    }
                }

                await ProcessDirectoryRecursivelyAsync(subdirectory, cancellationToken);
            }
        }

        private IEnumerable<DirectoryInfo> GetIncludedFilteredDirectoriesList(DirectoryInfo directoryPath)
        {
            var excludeList = new List<string>();
            foreach (var exclude in _settings.ExcludedDirectories)
            {
                excludeList.Add(directoryPath + exclude);
            }

            // Build a list of the Excluded directores using the "parent path"
            var projectDirectoriesEnum = directoryPath
                .EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                .Where(item => !excludeList.Contains(item.FullName, StringComparer.OrdinalIgnoreCase))
                ?? null;

            return projectDirectoriesEnum!;
        }

        private List<FileInfo> GetIncludedFilteredFilesList(DirectoryInfo directoryPath)
        {
            // 1. Optimize lookup and fix case-sensitivity (".JSON" vs ".json")
            // Using a HashSet makes the lookup O(1) instead of O(N), which matters for thousands of files.
            var allowedExtensions = new HashSet<string>(
                _settings.AllTargetExtensions,
                StringComparer.OrdinalIgnoreCase);

            // 2. Safely grab your new exclusion list (assuming you named it ExcludedFileNames)
            var excludedNames = _settings.ExcludeFilesContaining ?? new List<string>();

            // 3. Chain the filters for maximum readability and single-pass execution
            var filteredFiles = directoryPath.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                // Rule 1: Must be an allowed extension
                .Where(f => allowedExtensions.Contains(f.Extension))
                // Rule 2: Must NOT contain any of the excluded strings in its file name
                .Where(f => !excludedNames.Any(exclusion =>
                    f.Name.Contains(exclusion, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return filteredFiles;
        }
    }
}