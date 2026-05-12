using System.Text;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Converters;
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
            string? convertedPdfFile;
            //  0.  Determine the size of the file
            //  Use streaming for large file sizes
            var fileInfo = new FileInfo(filePath);

            //  size in bytes
            long fileBytes = fileInfo.Length;
            var maxNonStreamingFileBytes = 104857600;

            //  1.  Use MarkItDownNet package to convert to markdown
            var pdfConverter = new PdfConverter();

            if (fileBytes <= maxNonStreamingFileBytes)
            {
                convertedPdfFile = await pdfConverter.ConvertAsync(filePath);
            }
            else
            {
                var chunksStringBuilder = new StringBuilder();
                using var stream = File.OpenRead(filePath);

                await foreach (var chunk in pdfConverter.ConvertStreamingAsync(stream, ".pdf"))
                {
                    chunksStringBuilder.Append(chunk);
                }

                convertedPdfFile = chunksStringBuilder.ToString();
            }

            //  2.  Create a temp directory to store the file and read from
            var fileName = Path.GetFileName(filePath);

            //  re-use fileInfo that was previously retrieved
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