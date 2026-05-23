using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Processors
{
    public class ConfigConverterStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<ConfigConverterStrategy> _logger;

        public ConfigConverterStrategy(
            IFileConverterUtility fileConverter,
            IOptions<CodebaseScanSettings> options,
            ILogger<ConfigConverterStrategy> logger
        )
        {
            _fileConverter = fileConverter;
            _settings = options.Value;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _settings.ConfigExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> ConvertForIngestionAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}