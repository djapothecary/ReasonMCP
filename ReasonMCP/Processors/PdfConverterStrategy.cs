using ElBruno.MarkItDotNet;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Processors
{
    public class PdfConverterStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;
        private readonly ILogger<PdfConverterStrategy> _logger;

        public PdfConverterStrategy(
            IFileConverterUtility fileConverter,
            ILogger<PdfConverterStrategy> logger
        )
        {
            _fileConverter = fileConverter;
            _logger = logger;
        }
        public bool CanConvert(string filePath)
        {
            //  Ensure that the file is a PDF (*.pdf) file
            if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public async Task<bool> ConvertToMarkdownAsync(string filePath)
        {
            //  1.  Use MarkItDownNet package to convert to markdown
            var pdfConverter = new MarkdownConverter();
            var convertedPdfFile = pdfConverter.ConvertToMarkdown(filePath);

            //  2.  Create a temp directory to store the file and read from
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);
            var directoryPath = fileInfo.DirectoryName;
            var tempFileRoot = directoryPath + @"\Temp";
            var tempFilePath = Path.Combine(tempFileRoot, fileName.Replace(".pdf", ".md"));

            if (!Directory.Exists(tempFileRoot))
            {
                Directory.CreateDirectory(tempFileRoot);
            }

            File.WriteAllText(tempFilePath, convertedPdfFile);

            return await _fileConverter.ConvertToMarkdown(tempFilePath);
        }
    }
}