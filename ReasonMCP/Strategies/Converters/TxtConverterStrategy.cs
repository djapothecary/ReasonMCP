using ReasonMCP.Interfaces;

namespace ReasonMCP.Strategies.Converters
{
    public class TxtConverterStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;

        public TxtConverterStrategy(
            IFileConverterUtility fileConverter
        )
        {
            _fileConverter = fileConverter;
        }
        public bool CanConvert(string filePath)
        {
            //  Ensure that the file is a text (.txt) file
            if (filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public async Task<bool> ConvertForIngestionAsync(string filePath)
        {
            // no additional processing required, go straight to conversion
            return await _fileConverter.ConvertToMarkdown(filePath);
        }
    }
}