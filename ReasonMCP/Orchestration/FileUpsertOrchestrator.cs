using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ReaconMCP.Interfaces;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Utilities;

namespace ReasonMCP.Orchestration
{
    public class FileUpsertOrchestrator
    {
        private readonly IFileIngestionService _ingestService;
        private readonly IChunkParsingUtility _chunkParser;

        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly IOptions<StorageConfig> _options;
        private readonly ILogger<FileUpsertOrchestrator> _logger;

        public FileUpsertOrchestrator(
            IFileIngestionService ingestService,
            IChunkParsingUtility chunkParser,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IOptions<StorageConfig> options,
            ILogger<FileUpsertOrchestrator> logger
        )
        {
            _ingestService = ingestService;
            _chunkParser = chunkParser;
            _embeddingGenerator = embeddingGenerator;
            _options = options;
            _logger = logger;
        }

        public async Task ScanMarkdownDirectory(
            CancellationToken cancellationToken = default
        )
        {
            var baseDirectory = _options.Value.KnowledgeBaseRootDirectory;

            //  1.  Get all the Markdown directories
            //  All of the files that we want to process have been converted and
            //  moved to the relevant "Markdown" directory
            var subMarkdownDirectories = new DirectoryInfo(baseDirectory)
                .EnumerateDirectories("Markdowns", SearchOption.AllDirectories);

            foreach (var dir in subMarkdownDirectories)
            {
                _logger.LogTrace($"Preparing to Upsert file: {dir.Name}");

                await GetFileForUpsertAsync(dir.ToString(), cancellationToken);

                _logger.LogTrace("Upsert complete");
            }
        }

        /// <summary>
        /// Gets the file(s) from the directory path (filePath)
        /// and selects individual files for upsert
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task GetFileForUpsertFromIngestQueueAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            var convertedMarkdownPath = await ConvertToMarkdownPathAsync(filePath);
            var recordsToLoad = new List<KnowledgeRecord>();

            //  record Upsert Success status.  If there was a failure
            //  original markdowns will not be moved
            bool upsertSuccess = false;

            //  2.  Send file off to upsert
            _logger.LogTrace("Converting file for Vector database ...");

            recordsToLoad.AddRange(await _chunkParser.ParseEnrichedMarkdownAsync(convertedMarkdownPath, cancellationToken));

            _logger.LogTrace("List of records to upsert built successfully.");

            foreach (var record in recordsToLoad)
            {
                _logger.LogTrace($"Upserting record to vector store");

                if (string.IsNullOrWhiteSpace(record.Text))
                    continue;

                var generatedEmbeddings = await _embeddingGenerator.GenerateAsync(
                    new[] { record.Text },
                    cancellationToken: cancellationToken
                );

                record.Vector = generatedEmbeddings.First().Vector;

                upsertSuccess = await _ingestService.IngestSingleEnrichedObjectAsync(record, cancellationToken);

                _logger.LogTrace("Record successfully upserted");
            }

            if (upsertSuccess)
                await MoveMarkdownsToProcessedAsync();
        }

        /// <summary>
        /// Gets the file(s) from the directory path (filePath)
        /// and selects individual files for upsert
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task GetFileForUpsertAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            //  1.  Get all the files in the directory
            string[] files = Directory.GetFiles(filePath);
            var recordsToLoad = new List<KnowledgeRecord>();

            //  record Upsert Success status.  If there was a failure
            //  original markdowns will not be moved
            bool upsertSuccess = false;

            //  2.  Send files off to upsert
            foreach (var file in files)
            {
                _logger.LogTrace("Converting file for Vector database ...");

                recordsToLoad.AddRange(await _chunkParser.ParseEnrichedMarkdownAsync(file, cancellationToken));

                _logger.LogTrace("List of records to upsert built successfully.");
            }

            foreach (var record in recordsToLoad)
            {
                _logger.LogTrace($"Upserting record to vector store");

                if (string.IsNullOrWhiteSpace(record.Text))
                    continue;

                var generatedEmbeddings = await _embeddingGenerator.GenerateAsync(
                    new[] { record.Text },
                    cancellationToken: cancellationToken
                );

                record.Vector = generatedEmbeddings.First().Vector;

                upsertSuccess = await _ingestService.IngestSingleEnrichedObjectAsync(record, cancellationToken);

                _logger.LogTrace("Record successfully upserted");
            }

            if (upsertSuccess)
                await MoveMarkdownsToProcessedAsync();
        }

        private async Task MoveMarkdownsToProcessedAsync()
        {
            var baseDirectory = _options.Value.KnowledgeBaseRootDirectory;

            //  1.  Flatten the directory and file queries into a single, linear enumerable
            var filesToMove = new DirectoryInfo(baseDirectory)
                .EnumerateDirectories("Markdowns", SearchOption.AllDirectories)
                .SelectMany(dir => dir.EnumerateFiles("*.md"));

            //  2.  Use a single clean loop
            foreach (var file in filesToMove)
            {
                //  Safely construct the target path
                //  file.Directory is the "Markdowns" folder.
                //  file.Directory.Parent is the Topic Folder (e.g., "ADRs")
                var topicDirectory = file.Directory!.Parent!.FullName;
                var targetDirectory = Path.Combine(topicDirectory, "Processed");
                var targetFilePath = Path.Combine(targetDirectory, file.Name);

                //  Ensure the "Processed" directory actually exists
                //  this does nothing if the directory already exists
                Directory.CreateDirectory(targetDirectory);

                //  Move the file, the 'true' flag ensures we safely overwrite if the file being re-processed.
                file.MoveTo(targetFilePath, overwrite: true);
            }
        }



        private async Task<string> ConvertToMarkdownPathAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);
            var directoryPath = fileInfo.DirectoryName;  //  get the full path

            //  strip off "Temp" if this file was staged due to additional processing
            if (directoryPath!.EndsWith(@"\Temp"))
            {
                directoryPath = directoryPath.Replace(@"\Temp", "");
            }

            string? folderNameOnly;
            if (fileInfo?.Directory?.Name == "Temp")
            {
                folderNameOnly = fileInfo?.Directory?.Parent?.Name;
            }
            else
            {
                folderNameOnly = fileInfo?.Directory?.Name;   //  just the name of the containing folder
            }

            var convertedOutputRoot = directoryPath + @"\Markdowns";
            var convertedOutputPath = Path.Combine(convertedOutputRoot, fileName.Replace(".txt", ".md"));

            return convertedOutputPath;
        }
    }
}