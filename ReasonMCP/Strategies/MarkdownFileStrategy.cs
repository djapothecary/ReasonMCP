using ReasonMCP.Interfaces;

namespace ReasonMCP.Strategies
{
    /// <summary>
    /// This class is SPECIFICALLY it's own unique class.
    /// These files are already in Markdown format and will skipp conversion
    /// The files will move directly onto chunking/enrichment
    /// </summary>
    public class MarkdownFileStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;

        public MarkdownFileStrategy(
            IFileConverterUtility fileConverter
        )
        {
            _fileConverter = fileConverter;
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
            // this file is already a markdown file, move straight to chunking/enrichment
            return true;
        }
    }
}