using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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

        public async Task<bool> ConvertToMarkdown(string filePath)
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


            string rawText = await File.ReadAllTextAsync(filePath);
            var chunkUtility = new RAGChunkingUtility();

            var chunks = new List<string>();
            await foreach (var chunk in chunkUtility.CreateChunks(rawText))
            {
                chunks.Add(chunk);
            }

            //  Add Metadata enrichment
            var metadataEnricher = new MetadataEnrichmentUtility();
            var enrichedRagObj = metadataEnricher.EnrichChunksAsync(chunks, fileName);

            //  build version information
            string defaultVersion = "1.0";
            var versionMatch = Regex.Match(fileInfo!.Name, @"(?<version>\d\d+\.\d\d+)");
            if (versionMatch.Success)
            {
                defaultVersion = versionMatch.Groups["version"].Value;
            }

            var markdownBuilder = new StringBuilder();
            markdownBuilder.AppendLine("RAG Ingestion Data");
            markdownBuilder.AppendLine($"> **Source:**  {fileName}");
            markdownBuilder.AppendLine($"> **Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            markdownBuilder.AppendLine($"> **Topic:** {folderNameOnly}");
            markdownBuilder.AppendLine($"> **Header Context:** {enrichedRagObj.Result[0].SourceHeader}");
            markdownBuilder.AppendLine($"> **Generated Date:** {DateTime.UtcNow.ToString()}");
            markdownBuilder.AppendLine($"> **Version:** {defaultVersion}\n");

            for (int i = 0; i < enrichedRagObj.Result.Count; i++)
            {
                markdownBuilder.AppendLine($"## Chunk {i + 1}");
                markdownBuilder.AppendLine("```text");
                markdownBuilder.AppendLine(enrichedRagObj.Result[i].Content);
                markdownBuilder.AppendLine("```");
                markdownBuilder.AppendLine("\n---\n");
            }

            try
            {

                await File.WriteAllTextAsync(convertedOutputPath, markdownBuilder.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FILE_CONVERSION_ERROR].  An error occured converting {fileName} to Markdown.", fileName);
                return false;
            }
        }

        public async Task ClearOriginalFile(string filePath)
        {
            //  Delete the original (source) file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}