using ReasonMCP.Interfaces;

namespace ReasonMCP.Strategies.Converters
{
    /// <summary>
    /// This class is SPECIFICALLY it's own unique class.
    /// These files are already in Markdown format and will skipp conversion
    /// The files will move directly onto chunking/enrichment
    /// </summary>
    public class MarkdownFileStrategy : IFileConverterStrategy
    {
        private readonly IIngestionQueueService _ingestionQueue;

        public MarkdownFileStrategy(
            IIngestionQueueService ingestionQueue
        )
        {
            _ingestionQueue = ingestionQueue;
        }

        public bool CanConvert(string filePath)
        {
            //  Ensure that th efile is a Markdown (.md) file
            if (filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public async Task<bool> ConvertForIngestionAsync(
            string filePath,
            CancellationToken cancellationToken
        )
        {
            await _ingestionQueue.MarkConversionCompleteAsync(filePath, cancellationToken);

            // this file is already a markdown file, move straight to chunking/enrichment
            return true;
        }
    }
}