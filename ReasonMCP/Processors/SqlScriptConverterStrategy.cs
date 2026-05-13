using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Processors
{
    /// <summary>
    /// This classintentionally uses Boilerplate constructors
    /// </summary>
    public class SqlScriptConverterStrategy : IFileConverterStrategy
    {
        private readonly IFileConverterUtility _fileConverter;
        private readonly ILogger<SqlScriptConverterStrategy> _logger;

        private static readonly string[] _supportedExtensions =
        [
            ".sql"
        ];

        public SqlScriptConverterStrategy(
            IFileConverterUtility fileConverter,
            ILogger<SqlScriptConverterStrategy> logger
        )
        {
            _fileConverter = fileConverter;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            return _supportedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> ConvertToMarkdownAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}