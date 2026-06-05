using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IIngestionQueueService _ingestionQueue;

        public MarkdownFileStrategy(
            IServiceScopeFactory scopeFactory,
            IIngestionQueueService ingestionQueue
        )
        {
            _scopeFactory = scopeFactory;
            _ingestionQueue = ingestionQueue;
        }

        public bool CanConvert(string filePath)
        {
            //  Ensure that th efile is a Markdown (.md) file
            if (filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public async Task<bool> ConvertForIngestionAsync(string filePath)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var cancellationToken = cts.Token;

            await _ingestionQueue.MarkConversionCompleteAsync(filePath, cancellationToken);

            // this file is already a markdown file, move straight to chunking/enrichment
            return true;
        }
    }
}