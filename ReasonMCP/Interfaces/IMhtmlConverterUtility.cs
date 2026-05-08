using System.Threading;
using System.Threading.Tasks;

namespace ReasonMCP.Interfaces
{
    public interface IMhtmlConverterUtility
    {
        /// <summary>
        /// Parses an MHTML file, extracts the textual content, strips HTML formatting
        /// </summary>
        Task<string> ConvertToTextAsync(string filePath, CancellationToken cancellationToken = default);
        Task<string?> ExtractHtmlFromMhtmlAsync(string filePath, CancellationToken cancellationToken = default);
    }
}