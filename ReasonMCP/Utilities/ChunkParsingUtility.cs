using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Utilities
{
    public class ChunkParsingUtility : IChunkParsingUtility
    {
        public async Task<List<KnowledgebaseRecord>> ParseEnrichedMarkdownAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            //  1.  Read the entire file.  Enriched Markdown files are small enough
            //  that this is safe
            var fullText = string.Empty;

            //  Null safety check
            //  TODO:   Enhancement:    add better logging, null handling
            if (!File.Exists(filePath))
                return new List<KnowledgebaseRecord>();

            await File.ReadAllTextAsync(filePath, cancellationToken);

            //  2.  Structurally split the document by the chunk header.
            //  Sections[0] will ALWAYS be the Header MEtadata.
            //  Sections[1 ... n] will be the individual chunks.
            var sections = fullText.Split("## Chunk ", StringSplitOptions.RemoveEmptyEntries);

            if (sections.Length == 0)
                return new List<KnowledgebaseRecord>();

            //  3.  Extract Metadata ONCE from the header block
            var headerBlock = sections[0];
            string source = ExtractMetadata(headerBlock, "Source");
            string topic = ExtractMetadata(headerBlock, "Topic");
            string headerContext = ExtractMetadata(headerBlock, "Header Context");
            string generatedDate = ExtractMetadata(headerBlock, "Generated Date");
            string version = ExtractMetadata(headerBlock, "Version");

            var records = new List<KnowledgebaseRecord>();

            //  4.  Process the remaining blocks using a simple mapping loop
            for (int i = 1; i < sections.Length; i++)
            {
                var chunkBlock = sections[i];

                records.Add(new KnowledgebaseRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Source = source,
                    Topic = topic,
                    HeaderContext = headerContext,
                    ChunkIndex = ExtractChunkIndex(chunkBlock),
                    Text = ExtractFencedContent(chunkBlock),
                    GeneratedDate = generatedDate,
                    Version = version
                });
            }

            return records;
        }

        private static string ExtractMetadata(string textBlock, string key)
        {
            string marker = $"> **{key}:**";

            //  Find the line containing the marker, or return an empty string
            var line = textBlock.Split('\n').FirstOrDefault(l => l.Contains(marker));

            return line == null ? string.Empty : line.Substring(line.IndexOf(marker) + marker.Length).Trim();
        }

        private static int ExtractChunkIndex(string chunkBlock)
        {
            //  The index is the first thing in the chunk block (e.g., "1\n```text...")
            var firstLine = chunkBlock.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            int.TryParse(firstLine, out int index);
            return index;
        }

        private static string ExtractFencedContent(string chunkBlock)
        {
            int startMarker = chunkBlock.IndexOf("```text");
            int endMarker = chunkBlock.LastIndexOf("```");

            //  Bail out if the fences are missing or malformed
            if (startMarker == -1 || endMarker == -1 || endMarker <= startMarker)
                return string.Empty;

            startMarker += 7; //    move pat the "```text" string

            return chunkBlock.Substring(startMarker, endMarker - startMarker).Trim();

            //  simplified version
            //  return chunkBlock[startMarker..endMarker].Trim();
        }
    }
}