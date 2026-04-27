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
    public class FileIngestOrchestrator
    {
        private readonly IEnumerable<IDocumentProcessor> _processors;
        private readonly IFileConverterUtility _fileConverter;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly VectorStore _vectorStore;
        private readonly IOptions<StorageConfig> _options;
        private readonly ILogger<FileIngestOrchestrator> _logger;

        public FileIngestOrchestrator(
            IEnumerable<IDocumentProcessor> processors,
            IFileConverterUtility fileConverter,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            VectorStore vectorStore,
            IOptions<StorageConfig> options,
            ILogger<FileIngestOrchestrator> logger
            )
        {
            _processors = processors;
            _fileConverter = fileConverter;
            _embeddingGenerator = embeddingGenerator;
            _vectorStore = vectorStore;
            _options = options;
            _logger = logger;
        }

        public async Task IngestFileAsync(string filePath)
        {
            //  pre-processing moved files into a Markdown directory, so now the filepath needs to be updated
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);
            var directoryPath = fileInfo.DirectoryName;
            var markdownOutputRoot = directoryPath + @"\Markdowns";
            var markdownOutputPath = Path.Combine(markdownOutputRoot, fileName.Replace(".md", ".md"));


            //  1.  Ask all the processors: "who knows how to read this?"
            var processor = _processors.FirstOrDefault(p => p.CanProcess(markdownOutputPath));

            if (processor == null)
            {
                _logger.LogWarning("[FILE_INGEST_ERROR] No processor found for {file}", filePath);
                return;
            }

            var collection = _vectorStore.GetCollection<string, KnowledgeRecord>("ReasonContext");

            //  2.  The specific processor handles the chunking and hardcodes it's metadata tags
            var parsedChunks = await processor.ProcessAsync(filePath);

            var recordsToSave = new List<KnowledgeRecord>();

            foreach (var chunk in parsedChunks)
            {
                //  3.  get vectors from Nomic
                var embedding = await _embeddingGenerator.GenerateAsync(chunk.Text);

                //  4.  Assemble the FINAL, fully hydrated Database entity
                recordsToSave.Add(new KnowledgeRecord
                {
                    Text = chunk.Text,
                    Topic = chunk.Topic,
                    Source = chunk.Source,
                    ChunkIndex = chunk.ChunkIndex,
                    Metadata = chunk.Metadata,
                    Vector = embedding.Vector
                });
            }

            //  5.  Save to SQLite
            await collection.UpsertAsync(recordsToSave);
        }
    }
}