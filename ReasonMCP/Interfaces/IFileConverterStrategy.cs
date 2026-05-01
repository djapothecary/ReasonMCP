namespace ReasonMCP.Interfaces
{
    public interface IFileConverterStrategy
    {
        bool CanConvert(string filePath);
        Task<bool> ConvertToMarkdownAsync(string filePath);
    }
}