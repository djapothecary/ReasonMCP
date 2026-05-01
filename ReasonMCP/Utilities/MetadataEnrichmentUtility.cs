using System.Text.RegularExpressions;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Utilities
{
    public class MetadataEnrichmentUtility : IMetadataEnrichmentUtility
    {
        public async Task<List<RagObject>> EnrichChunksAsync(IEnumerable<string> chunks, string sourceName)
        {
            var enrichedList = new List<RagObject>();
            string currentHeader = "General Content";
            int index = 0;

            //  RegEx to find Markdown headers (e.g. # Header ## Subheader)
            var headerRegEx = new Regex(@"^#+\s\s+(.*)$", RegexOptions.Multiline);

            foreach (var chunkText in chunks)
            {
                //  1.  Context tracking: look for headers inside this chunk
                var headerMatch = headerRegEx.Match(chunkText);

                if (headerMatch.Success)
                {
                    //  Update the current "context" based on the last header found
                    currentHeader = headerMatch.Groups[1].Value.Trim();
                }

                //  2.  Build the RagObject
                var ragObj = new RagObject
                {
                    Content = chunkText.ToString(),
                    SourceHeader = currentHeader,
                    ChunkIndex = index++,
                    Metadata = new Dictionary<string, object>
                    {
                        { "source", sourceName },
                        { "header_context", currentHeader },
                        { "processed_at", DateTime.Now },
                        { "chunk_sequence", index }
                    }
                };

                enrichedList.Add(ragObj);
            }

            return enrichedList;
        }
    }
}