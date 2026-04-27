using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ModelContextProtocol.Protocol;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Utilities;

namespace ReasonMCP.Orchestration
{
    public class PreProcessOrchestrator
    {
        private readonly IEnumerable<IDocumentProcessor> _processors;
        private readonly IFileConverterUtility _fileConverter;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly VectorStore _vectorStore;
        private readonly IOptions<StorageConfig> _options;
        private readonly ILogger<PreProcessOrchestrator> _logger;

        public PreProcessOrchestrator(
            IEnumerable<IDocumentProcessor> processors,
            IFileConverterUtility fileConverter,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            VectorStore vectorStore,
            IOptions<StorageConfig> options,
            ILogger<PreProcessOrchestrator> logger
            )
        {
            _processors = processors;
            _fileConverter = fileConverter;
            _embeddingGenerator = embeddingGenerator;
            _vectorStore = vectorStore;
            _options = options;
            _logger = logger;
        }

        public async Task ScanDirectory(CancellationToken cancellationToken)
        {
            var baseDirectory = _options.Value.KnowledgeBaseRootDirectory;

            var childDirectoriesEnum = new DirectoryInfo(baseDirectory)
                .EnumerateDirectories("*", SearchOption.TopDirectoryOnly);

            foreach (var dir in childDirectoriesEnum)
            {
                _logger.LogTrace($"Processing file: {dir.Name}");

                await PreprocessFileAsync(dir.ToString());

                _logger.LogTrace("Processing completed.");
            }

            //  now get the markdown directories and begin processing

        }

        public async Task PreprocessFileAsync(string filePath)
        {
            //  1.  Get all the files in the directory
            string[] files = Directory.GetFiles(filePath);

            //  2.  Pre-processing: Convert files to markdown
            foreach (var file in files)
            {
                if (file.EndsWith(".txt"))
                {
                    await _fileConverter.ConvertTextToMarkdown(file);
                }

                if (file.EndsWith(".mhtml"))
                {
                    await _fileConverter.ConvertHtmlToMarkdown(file);
                }
            }

            //  TODO:   Feature:    Add Converter/Processor for images
            //  will need an image model (moondream2) if this becomes necessary
        }
    }
}