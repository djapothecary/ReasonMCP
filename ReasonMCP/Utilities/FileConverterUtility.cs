using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Utilities;

namespace ReasonMCP.Utilities
{
    public class FileConverterUtility : IFileConverterUtility
    {
        private readonly IOptions<StorageConfig> _options;
        private readonly ILogger<FileConverterUtility> _logger;

        public FileConverterUtility(
            IOptions<StorageConfig> options,
            ILogger<FileConverterUtility> logger
        )
        {
            _options = options;
            _logger = logger;
        }

        public async Task ConvertTextToMarkdown(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);
            var directoryPath = fileInfo.DirectoryName;  //  get the full path
            var folderNameOnly = fileInfo?.Directory?.Name;   //  just the name of the containing folder

            var convertedOutputRoot = directoryPath + @"\Markdowns";
            var convertedOutputPath = Path.Combine(convertedOutputRoot, fileName.Replace(".txt", ".md"));

            string rawText = await File.ReadAllTextAsync(filePath);
            var chunkUtility = new RAGChunkingUtility();

            var chunks = new List<string>();
            await foreach (var chunk in chunkUtility.CreateChunks(rawText))
            {
                chunks.Add(chunk);
            }

            var markdownBuilder = new StringBuilder();
            markdownBuilder.AppendLine("RAG Ingestion Data");
            markdownBuilder.AppendLine($"> **Source:**  {fileName}");
            markdownBuilder.AppendLine($"> **Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            for (int i = 0; i < chunks.Count; i++)
            {
                markdownBuilder.AppendLine($"## Chunk {i + 1}");
                markdownBuilder.AppendLine("```text");
                markdownBuilder.AppendLine(chunks[i]);
                markdownBuilder.AppendLine("```");
                markdownBuilder.AppendLine("\n---\n");
            }

            await File.WriteAllTextAsync(convertedOutputPath, markdownBuilder.ToString());
        }

        public Task ConvertHtmlToMarkdown(string filePath)
        {
            throw new NotImplementedException();
            // var fileName = Path.GetFileName(filePath);
            // var fileInfo = new FileInfo(filePath);
            // var directoryPath = fileInfo.DirectoryName;  //  get the full path
            // var folderNameOnly = fileInfo?.Directory?.Name;   //  just the name of the containing folder

            // var convertedOutputRoot = directoryPath + @"\Markdowns";
            // var convertedOutputPath = Path.Combine(convertedOutputRoot, fileName.Replace(".mhtml", ".md"));
        }
    }
}