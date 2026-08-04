namespace ReasonMCP.Interfaces
{
    public interface IFileConverterUtility
    {
        Task<bool> ConvertToMarkdown(
            string filePath,
            CancellationToken cancellationToken
        );

        Task<bool> ChunkExistingMarkdown(
            string filePath,
            CancellationToken cancellationToken
        );

        Task ClearOriginalFile(string filePath);
    }
}