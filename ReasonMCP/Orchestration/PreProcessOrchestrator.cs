using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Utilities;

namespace ReasonMCP.Orchestration
{
    public class PreProcessOrchestrator
    {
        private readonly IEnumerable<IFileConverterStrategy> _strategies;
        private readonly IFileConverterUtility _fileConverter;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly VectorStore _vectorStore;
        private readonly StorageConfig _options;
        private readonly ILogger<PreProcessOrchestrator> _logger;

        public PreProcessOrchestrator(
            IEnumerable<IFileConverterStrategy> strategies,
            IFileConverterUtility fileConverter,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            VectorStore vectorStore,
            IOptions<StorageConfig> options,
            ILogger<PreProcessOrchestrator> logger
            )
        {
            _strategies = strategies;
            _fileConverter = fileConverter;
            _embeddingGenerator = embeddingGenerator;
            _vectorStore = vectorStore;
            _options = options.Value;
            _logger = logger;
        }

        public async Task ScanDirectory(CancellationToken cancellationToken)
        {
            var baseDirectory = _options.KnowledgeBaseRootDirectory;

            var childDirectoriesEnum = new DirectoryInfo(baseDirectory)
                .EnumerateDirectories("*", SearchOption.TopDirectoryOnly) ?? null;

            if (childDirectoriesEnum != null)
            {
                foreach (var dir in childDirectoriesEnum)
                {
                    _logger.LogTrace($"Processing file: {dir.Name}");

                    if (!string.IsNullOrEmpty(dir.ToString()))
                    {
                        await PreprocessFileAsync(dir.ToString());
                    }

                    _logger.LogTrace("Processing completed.");
                }
            }
        }

        public async Task PreprocessFileFromQueueAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            bool convertSuccesss;

            //  2.1 Determine file type and what processor to use
            var strategy = _strategies.FirstOrDefault(s => s.CanConvert(filePath));

            //  3.  Convert file to markdown
            convertSuccesss = await strategy!.ConvertToMarkdownAsync(filePath);

            if (convertSuccesss)
                await _fileConverter.ClearOriginalFile(filePath);

            //  TODO:   Feature:    Add Converter/Processor for images
            //  will need an image model (moondream2) if this becomes necessary
        }

        public async Task PreprocessFileAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            //  1.  Get all the files in the directory
            string[] files = Directory.GetFiles(filePath);

            //  2.  Pre-processing: Convert files to markdown
            //  Prevent exception of empty files
            if (files.Length != 0)
            {
                foreach (var file in files)
                {
                    bool convertSuccesss;

                    //  2.1 Determine file type and what processor to use
                    var strategy = _strategies.FirstOrDefault(s => s.CanConvert(file));

                    //  3.  Convert file to markdown
                    convertSuccesss = await strategy!.ConvertToMarkdownAsync(file);

                    if (convertSuccesss)
                        await _fileConverter.ClearOriginalFile(file);
                }
            }

            //  TODO:   Feature:    Add Converter/Processor for images
            //  will need an image model (moondream2) if this becomes necessary
        }
    }
}