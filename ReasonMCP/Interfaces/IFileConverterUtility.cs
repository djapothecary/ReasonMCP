namespace ReasonMCP.Interfaces
{
    public interface IFileConverterUtility
    {
        Task<bool> ConvertToMarkdown(string filePath);
        Task ClearOriginalFile(string filePath);
    }
}