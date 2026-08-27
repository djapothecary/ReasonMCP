using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;
using ReasonMCP.Models;
using Spectre.Console;

namespace ReasonMCP.Processors
{
    public class DocumentsProcessor : IDocumentsProcessor
    {
        private readonly IChunkParsingUtility _chunkParser;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestEnrichedRecordsService _ingestService;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly IOptionsMonitor<StorageConfigSettings> _settings;
        private readonly ILogger<DocumentsProcessor> _logger;

        public DocumentsProcessor(
            IChunkParsingUtility chunkParser,
            IIngestionQueueService ingestionQueue,
            IIngestEnrichedRecordsService ingestService,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IOptionsMonitor<StorageConfigSettings> options,
            ILogger<DocumentsProcessor> logger
        )
        {
            _chunkParser = chunkParser;
            _ingestionQueue = ingestionQueue;
            _ingestService = ingestService;
            _embeddingGenerator = embeddingGenerator;
            _settings = options;
            _logger = logger;
        }

        /// <summary>
        /// Dequeue the next Documents Data record from
        /// IngestionQueue
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileIngestionRecord> GetNextDocumentFileAsync(
            CancellationToken cancellationToken
        )
        {
            var file = await _ingestionQueue.DequeueNextFileAsync(
                "Documents",
                cancellationToken
            );

            await Task.Delay(500, cancellationToken);
            return file!;
        }

        /// <summary>
        /// Gets the file(s) from the directory path (filePath)
        /// and selects individual files for upsert
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<bool> IngestDocumentRecordAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            if (filePath == null)
                return false;

            //  TODO:   Refactor:   This could proably stand to be moved to it's own class
            var convertedToMarkdownPath = await ConvertToMarkdownPathAsync(filePath);
            var recorrdsToload = new List<DocumentVectorModel>();

            //  document Upsert Success status.  If there was a failure
            //  the original markdowns will not be moved
            bool upsertSuccess = false;

            //  #.  Send file off for conversion
            _logger.LogTrace("Converting file for Vector database ...");

            recorrdsToload.AddRange(
                await _chunkParser.ParseEnrichedDocumentMarkdownAsync(
                    convertedToMarkdownPath,
                    cancellationToken
                )
            );

            _logger.LogTrace("List of records to upsert built successfully.");

            foreach (var record in recorrdsToload)
            {
                _logger.LogTrace("Upserting record to vector store");

                if (string.IsNullOrWhiteSpace(record.Text))
                    continue;

                var generatedEmbeddings = await _embeddingGenerator.GenerateAsync(
                    new[]
                    {
                        record.Text
                    },
                    cancellationToken: cancellationToken
                );

                record.Vector = generatedEmbeddings.First().Vector;

                upsertSuccess = await _ingestService.IngestEnrichedDocumentRecordAsync(
                    record,
                    cancellationToken
                );

                int chunkCount = 1;
                AnsiConsole.WriteLine($"Successfully upserted {filePath} Chunk count: {chunkCount}", filePath, chunkCount);
                chunkCount++;

                _logger.LogTrace("Record successfully upserted");
            }

            //  TODO:   Refactor:   This could proably stand to be moved to it's own class
            if (upsertSuccess)
                await MoveMarkdownsToProcessedAsync();

            return upsertSuccess;
        }

        public async Task MoveMarkdownsToProcessedAsync()
        {
            var baseDirectories = _settings.CurrentValue.DocumentsBaseRootDirectories;

            foreach (var directory in baseDirectories)
            {
                //  1.  Flatten the directory and file queries into a single, linear enumerable
                var filesToMove = new DirectoryInfo(directory)
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
        }

        public async Task<string> ConvertToMarkdownPathAsync(
            string filePath
        )
        {
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);
            var fileExtension = fileInfo.Extension;
            var directoryPath = fileInfo.DirectoryName;  //  get the full path

            //  strip off "Temp" if this file was staged due to additional processing
            if (directoryPath!.EndsWith(@"\Temp"))
            {
                directoryPath = directoryPath.Replace(@"\Temp", "");
            }

            var convertedOutputRoot = directoryPath;
            var convertedOutputPath = Path.Combine(
                convertedOutputRoot,
                fileName.Replace(
                    fileExtension,
                    ".md"
                )
            );

            return convertedOutputPath;
        }
    }
}