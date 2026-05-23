using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;

namespace ReasonMCP.Processors
{
    public class MarkupConverterStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<MarkupConverterStrategy> _logger;

        public MarkupConverterStrategy(
            IFileConverterUtility fileConverter,
            IOptions<CodebaseScanSettings> options,
            ILogger<MarkupConverterStrategy> logger
        )
        {
            _fileConverter = fileConverter;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.MarkupExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> ConvertForIngestionAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}