using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;
using ReasonMCP.Models;

namespace ReasonMCP.Processors
{
    public class ReferenceDataProcessor : IReferenceDataProcessor
    {
        private readonly IChunkParsingUtility _chunkParser;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestEnrichedRecordsService _ingestService;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly IOptions<StorageConfigSettings> _options;
        private readonly ILogger<ReferenceDataProcessor> _logger;

        public ReferenceDataProcessor(
            IChunkParsingUtility chunkParser,
            IIngestionQueueService ingestionQueue,
            IIngestEnrichedRecordsService ingestService,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IOptions<StorageConfigSettings> options,
            ILogger<ReferenceDataProcessor> logger
        )
        {
            _chunkParser = chunkParser;
            _ingestionQueue = ingestionQueue;
            _ingestService = ingestService;
            _embeddingGenerator = embeddingGenerator;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Dequeue the next Reference Data record from
        /// IngestionQueue
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileIngestionRecord> GetNextReferenceFileAsync(
            CancellationToken cancellationToken
        )
        {
            var file = await _ingestionQueue.DequeueNextFileAsync(
                "Reference",
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
        public async Task<bool> IngestReferenceFileRecordAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            var convertedMarkdownPath = await ConvertToMarkdownPathAsync(filePath);
            var recordsToLoad = new List<ReferenceVectorModel>();

            //  record Upsert Success status.  If there was a failure
            //  original markdowns will not be moved
            bool upsertSuccess = false;

            //  2.  Send file off to upsert
            _logger.LogTrace("Converting file for Vector database ...");

            recordsToLoad.AddRange(
                await _chunkParser.ParseEnrichedReferenceMarkdownAsync(
                    convertedMarkdownPath,
                    cancellationToken
                )
            );

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

                upsertSuccess = await _ingestService.IngestEnrichedReferenceRecordAsync(
                    record,
                    cancellationToken
                );

                int chunkCount = 1;
                Console.WriteLine($"Successfully upserted {filePath}  Chunk Count: {chunkCount}", filePath, chunkCount);
                chunkCount++;

                _logger.LogTrace("Record successfully upserted");
            }

            //  Update ingestion status

            if (upsertSuccess)
                await MoveMarkdownsToProcessedAsync();

            return upsertSuccess;
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
            var recordsToLoad = new List<ReferenceVectorModel>();

            //  record Upsert Success status.  If there was a failure
            //  original markdowns will not be moved
            bool upsertSuccess = false;

            //  2.  Send files off to upsert
            foreach (var file in files)
            {
                _logger.LogTrace("Converting file for Vector database ...");

                recordsToLoad.AddRange(
                    await _chunkParser.ParseEnrichedReferenceMarkdownAsync(
                        file,
                        cancellationToken
                    )
                );

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

                upsertSuccess = await _ingestService.IngestEnrichedReferenceRecordAsync(
                    record,
                    cancellationToken
                );

                _logger.LogTrace("Record successfully upserted");

                Console.WriteLine($"Successfully upserted {record}", record);
            }

            if (upsertSuccess)
                await MoveMarkdownsToProcessedAsync();
        }

        public async Task MoveMarkdownsToProcessedAsync()
        {
            var baseDirectory = _options.Value.ReferenceBaseRootDirectory;

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