using DocumentFormat.OpenXml.Office.CustomUI;
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
    public class CodebaseScanOrchestrator
    {
        private readonly IEnumerable<IFileConverterStrategy> _strategies;
        private readonly IFileConverterUtility _fileConverter;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly VectorStore _vectorStore;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<CodebaseScanOrchestrator> _logger;

        public CodebaseScanOrchestrator(
            IEnumerable<IFileConverterStrategy> strategies,
            IFileConverterUtility fileConverter,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            VectorStore vectorStore,
            IOptions<CodebaseScanSettings> options,
            ILogger<CodebaseScanOrchestrator> logger
        )
        {
            _strategies = strategies;
            _fileConverter = fileConverter;
            _embeddingGenerator = embeddingGenerator;
            _vectorStore = vectorStore;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task ScanCodebaseAsync(CancellationToken cancellationToken)
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

                await ScanCodebaseProjectDirectoriesAsync(dir, cancellationToken);
            }
        }

        /// <summary>
        /// Scan for all of the "Project" files that are contained in the
        /// Parent "directoryPath" as provided from appsetings.json
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ScanCodebaseProjectDirectoriesAsync(
            DirectoryInfo directoryPath,
            CancellationToken cancellationToken
        )
        {
            // Build a list of the Excluded directores using the "parent path"
            var projectDirectoriesEnum = GetIncludedFilteredDirectoriesList(directoryPath) ?? null;

            if (projectDirectoriesEnum != null)
            {
                foreach (var projectDirectory in projectDirectoriesEnum)
                {
                    var projectDirectoryFiles = new List<FileInfo>();
                    _logger.LogTrace($"Processing Codebase Project directory: {projectDirectory.FullName}");

                    if (projectDirectory.GetFiles().Length > 0)
                    {
                        projectDirectoryFiles = GetIncludedFilteredFilesList(projectDirectory);

                        //  Handle any "straggler files" for processing here
                        //  Then move on to process sub directories
                    }

                    await GetProjectChildDirectoriesAsync(projectDirectory, cancellationToken);

                }
            }
        }

        public async Task GetProjectChildDirectoriesAsync(
            DirectoryInfo projectDirectory,
            CancellationToken cancellationToken
        )
        {
            var projectChildDirectoriesList = GetIncludedFilteredDirectoriesList(projectDirectory) ?? null;

            if (projectChildDirectoriesList != null)
            {
                foreach (var childDirectory in projectChildDirectoriesList)
                {
                    var childDirectoryFiles = new List<FileInfo>();
                    if (childDirectory.GetDirectories().Length == 0)
                    {
                        childDirectoryFiles = GetIncludedFilteredFilesList(childDirectory);

                        //  Handle any "straggler files" for processing here
                        //  Then move on to process sub directories
                    }

                    //  One final pass for any other sub ("composite") directories
                    if (childDirectory.GetDirectories().Length > 0)
                    {
                        var childSubDirectories = GetIncludedFilteredDirectoriesList(childDirectory) ?? null;
                        await GetChildCompositeDirectoriesAsync(childDirectory, cancellationToken);
                    }
                }
            }
        }

        public async Task GetChildCompositeDirectoriesAsync(
            DirectoryInfo childSubDirectory,
            CancellationToken cancellationToken
        )
        {
            var childCompositeDirectories = GetIncludedFilteredDirectoriesList(childSubDirectory) ?? null;

            if (childCompositeDirectories != null)
            {
                foreach (var compositeDirectory in childCompositeDirectories)
                {
                    var compositeDirectoryFiles = new List<FileInfo>();
                    if (compositeDirectory.GetDirectories().Length == 0)
                    {
                        compositeDirectoryFiles = GetIncludedFilteredFilesList(compositeDirectory);

                        //  Handle any "straggler files" for processing here
                        //  Then move on to process sub directories
                    }

                    //  One final pass for any other sub directories
                    if (compositeDirectory.GetDirectories().Length > 0)
                    {
                        await GetChildCompositeSubDirectoriesAsync(compositeDirectory, cancellationToken);
                    }
                }
            }
        }

        public async Task GetChildCompositeSubDirectoriesAsync(
            DirectoryInfo childCompositeDirectory,
            CancellationToken cancellationToken
        )
        {
            var childCompositeSubDirectories = GetIncludedFilteredDirectoriesList(childCompositeDirectory) ?? null;

            if (childCompositeSubDirectories != null)
            {
                foreach (var compositeDirectory in childCompositeSubDirectories)
                {
                    var childCompositeSubDirectoryFiles = new List<FileInfo>();
                    if (compositeDirectory.GetDirectories().Length == 0)
                    {
                        childCompositeSubDirectoryFiles = GetIncludedFilteredFilesList(compositeDirectory);

                        //  Handle any "straggler files" for processing here
                        //  Then move on to process sub directories
                    }

                    //  One final pass for any other sub directories
                    if (compositeDirectory.GetDirectories().Length > 0)
                    {
                        // We SHOULD be stopping here
                    }
                }
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
            foreach (var include in _settings.AllTargetExtensions)
            {
                includedList.Add(include);
            }

            var projectFilesEnum = directoryPath.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => includedList.Contains(f.Extension));

            return [.. projectFilesEnum];
        }
    }
}