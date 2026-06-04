using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Utilities
{
    public class FileConverterUtility : IFileConverterUtility
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICodeChunkingProcessor _configChunkingProcessor;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IIngestionQueueUpdaterService _updaterService;
        private readonly StorageConfigSettings _settings;
        private readonly ILogger<FileConverterUtility> _logger;

        public FileConverterUtility(
            IServiceScopeFactory scopeFactory,
            IIngestionQueueService ingestionQueue,
            IIngestionQueueUpdaterService updaterService,
            IOptions<StorageConfigSettings> options,
            ILogger<FileConverterUtility> logger
        )
        {
            _scopeFactory = scopeFactory;
            _ingestionQueue = ingestionQueue;
            _updaterService = updaterService;
            _settings = options.Value;
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

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            try
            {
                //  null safety
                if (!File.Exists(filePath))
                {
                    _logger.LogError("[FILE_CONVERSION_ERROR].  Path not found {fileName}.", fileName);
                    await _ingestionQueue.MarkConversionFailedAsync(filePath, "[FILE_CONVERSION_ERROR].  Path not found", cancellationToken);

                    return false;
                }

                await File.WriteAllTextAsync(convertedOutputPath, markdownBuilder.ToString());

                await _updaterService.MarkConversionStatus(filePath, true, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FILE_CONVERSION_ERROR].  An error occured converting {fileName} to Markdown.", fileName);
                await _ingestionQueue.MarkFailedExceptionAsync(filePath, ex.Message, cancellationToken);

                return false;
            }
        }

        public async Task<bool> ChunkExistingMarkdown(string filePath)
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

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            try
            {
                //  null safety
                if (!File.Exists(filePath))
                {
                    _logger.LogError("[FILE_CONVERSION_ERROR].  Path not found {fileName}.", fileName);
                    await _ingestionQueue.MarkConversionFailedAsync(filePath, "[FILE_CONVERSION_ERROR].  Path not found", cancellationToken);

                    return false;
                }

                await File.WriteAllTextAsync(convertedOutputPath, markdownBuilder.ToString());

                await _updaterService.MarkConversionStatus(filePath, true, cancellationToken);

                //  Clear the "temp" file
                await ClearOriginalFile(filePath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FILE_CONVERSION_ERROR].  An error occured converting {fileName} to Markdown.", fileName);
                await _ingestionQueue.MarkFailedExceptionAsync(filePath, ex.Message, cancellationToken);

                return false;
            }
        }

        public async Task ClearOriginalFile(string filePath)
        {
            //  Preserve GeminiConsolidated workspace files
            if (filePath.Contains("Gemini"))
                return;

            //  Delete the original (source) file
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}