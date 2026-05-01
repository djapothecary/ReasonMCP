using ReasonMCP.Interfaces;

namespace ReasonMCP.Processors
{
    public class MhtmlConverterStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;

        public MhtmlConverterStrategy(
            IFileConverterUtility fileConverter
        )
        {
            _fileConverter = fileConverter;
        }

        public bool CanConvert(string filePath)
        {
            //  Ensure that the file is a single web page (.mhtml) file
            if (filePath.EndsWith(".mhtml", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public async Task<bool> ConvertToMarkdownAsync(string filePath)
        {
            // MHTML will require additional processing before being sent to mark down

            return await _fileConverter.ConvertToMarkdown(filePath);
        }
    }
}