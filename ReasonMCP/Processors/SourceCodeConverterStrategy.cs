using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configuration;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Processors
{
    public class SourceCodeConverterStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<SourceCodeConverterStrategy> _logger;

        public SourceCodeConverterStrategy(
            IFileConverterUtility fileConverter,
            IOptions<CodebaseScanSettings> options,
            ILogger<SourceCodeConverterStrategy> logger
        )
        {
            _fileConverter = fileConverter;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.SourceCodeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

        }

        public Task<bool> ConvertToMarkdownAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}