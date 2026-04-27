namespace ReasonMCP.Interfaces
{
    public interface IFileConverterUtility
    {
        Task ConvertTextToMarkdown(string filePath);
        Task ConvertHtmlToMarkdown(string filePath);
    }
}