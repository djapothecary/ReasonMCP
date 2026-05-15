using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Configuration;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Utilities;

namespace ReasonMCP.Orchestration
{
    public class KnowledgebaseScanOrchestrator
    {
        private readonly KnowledgebaseScanSettings _settings;
        private readonly ILogger<KnowledgebaseScanOrchestrator> _logger;

        public KnowledgebaseScanOrchestrator(
            IOptions<KnowledgebaseScanSettings> options,
            ILogger<KnowledgebaseScanOrchestrator> logger
        )
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task ScanKnowledgebaseAsync(
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
                _logger.LogTrace($"Processing Knowledgebase directory: {dir.FullName}");
                await ProcessDirectoryRecursivelyAsync(dir, cancellationToken);
            }
        }

        public async Task ProcessDirectoryRecursivelyAsync(
            DirectoryInfo directory,
            CancellationToken cancellationToken = default
        )
        {
            var subDirectories = GetIncludedFilteredDirectoriesList(directory);

            if (subDirectories == null || !subDirectories.Any())
                return;

            foreach (var subdirectory in subDirectories)
            {
                _logger.LogTrace($"Processing Knowledgebase Document directory: {subdirectory.FullName}");

                var files = GetIncludedFilteredFilesList(subdirectory);
                if (files.Count > 0)
                {
                    //  Add files to IngestionQueue here
                    foreach (var file in files)
                    {

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
            var includedList = new List<string>();
            foreach (var include in _settings.AllKnowledgeExtensions)
            {
                includedList.Add(include);
            }

            var projectFilesEnum = directoryPath.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => includedList.Contains(f.Extension));

            return [.. projectFilesEnum];
        }
    }
}