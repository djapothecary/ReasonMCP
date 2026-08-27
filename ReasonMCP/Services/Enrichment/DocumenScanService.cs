using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces.IEnrichment;

namespace ReasonMCP.Services.Enrichment
{
    public class DocumentScanService : IDocumentScanService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<DocumentScanSettings> _settings;
        private readonly ILogger<DocumentScanService> _logger;

        public DocumentScanService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<DocumentScanSettings> options,
            ILogger<DocumentScanService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options;
            _logger = logger;
        }

        /// <summary>
        /// Scans Reference data locations based on Settings in
        /// referenceScanSettings.json
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ScanDocumentsAsync(
            CancellationToken cancellationToken = default
        )
        {
            foreach (var rootDirectory in _settings.CurrentValue.RootDirectories)
            {
                var rootDirInfo = new DirectoryInfo(rootDirectory);
                IEnumerable<DirectoryInfo> directoriesToScan;

                if (_settings.CurrentValue.SubDirectories.Count > 0)
                {
                    var dirList = new List<DirectoryInfo>();
                    foreach (var subDir in _settings.CurrentValue.SubDirectories)
                    {
                        //  Using Path.Combine for robustness, assuming subDir is relative to rootDirectory
                        var fullPath = Path.Combine(rootDirectory, subDir);
                        if (Directory.Exists(fullPath))
                        {
                            dirList.Add(new DirectoryInfo(fullPath));
                        }
                    }
                    directoriesToScan = dirList;
                }
                else
                {
                    directoriesToScan = rootDirInfo.EnumerateDirectories(
                        "*",
                        SearchOption.AllDirectories
                    );
                }

                foreach (var dir in directoriesToScan)
                {
                    _logger.LogTrace($"Processing Documents directory: {dir.FullName}");
                    await ProcessDirectoryRecursivelyAsync(
                        dir,
                        cancellationToken
                    );
                }
            }
        }

        /// <summary>
        /// Recursively process all Directories and Subdirectories
        /// files and directories/subdirectories are filtered based on
        /// settings in referenceScanSettings.json
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessDirectoryRecursivelyAsync(
            DirectoryInfo directory,
            CancellationToken cancellationToken = default
        )
        {
            var subDirectories = GetIncludedFilteredDirectoriesList(directory);

            if (subDirectories == null || !subDirectories.Any())
                return;

            using var scope = _scopeFactory.CreateScope();
            var dapperIngestionQueue = scope
                .ServiceProvider
                .GetRequiredService<DapperIngestionQueueService>();

            foreach (var subDirectory in subDirectories)
            {
                _logger.LogTrace($"Processing Documents directory: {subDirectory}");

                var files = GetIncludedFilteredFilesList(subDirectory);
                if (files.Count > 0)
                {
                    //  Add files to IngestionQueue
                    foreach (var file in files)
                    {
                        await dapperIngestionQueue.UpsertToQueueAsync(
                            file.FullName,
                            "Documents",
                            file.LastWriteTimeUtc,
                            cancellationToken
                        );

                        Console.WriteLine(file.FullName);
                    }
                }
            }
        }

        /// <summary>
        /// Filters out directories based on ExcludedDirectories setting in
        /// referenceScanSettings.json
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private IEnumerable<DirectoryInfo> GetIncludedFilteredDirectoriesList(
            DirectoryInfo directoryPath
        )
        {
            var excludedList = new List<string>();
            foreach (var exclude in _settings.CurrentValue.ExcludedDirectories)
            {
                excludedList.Add(directoryPath + exclude);
            }

            //  Build a list of the Excluded directories using the "parent path"
            var documentsDirectoriesEnum = directoryPath
                .EnumerateDirectories(
                    "*",
                    SearchOption.TopDirectoryOnly
                )
                .Where(item => !excludedList.Contains(
                    item.FullName,
                    StringComparer.OrdinalIgnoreCase)
                ) ?? null;

            return documentsDirectoriesEnum!;
        }

        /// <summary>
        /// Filters out (excludes) files by file extension setting in
        /// referenceScanSettings.json
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private List<FileInfo> GetIncludedFilteredFilesList(
            DirectoryInfo directoryPath
        )
        {
            // 1. Optimize lookup and fix case-sensitivity (".JSON" vs ".json")
            // Using a HashSet makes the lookup O(1) instead of O(N), which matters for thousands of files.
            var allowedExtensions = new HashSet<string>(
                _settings.CurrentValue.AllReferenceExtensions,
                StringComparer.OrdinalIgnoreCase
            );

            //  2.  Safely grab the new exclusion list (assuming it is named ExcludedFileNames)
            var excludedNames = _settings
                .CurrentValue
                .ExcludeFilesContaining ?? new List<string>();

            //  3.  Chain the filters for maximum readability and single-pass execution
            var filteredFiles = directoryPath.EnumerateFiles(
                    "*.*",
                    SearchOption.TopDirectoryOnly
                )
                //  Rule 1: Must be allowed extension
                .Where(f => allowedExtensions.Contains(f.Extension))
                //  Rule 2: Must NOT contain any of the excluded strings in its file name
                .Where(f => !excludedNames.Any(exclusion =>
                    f.Name.Contains(
                        exclusion,
                        StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                .ToList();

            return filteredFiles;
        }
    }
}