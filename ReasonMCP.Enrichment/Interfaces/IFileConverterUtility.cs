namespace ReasonMCP.Enrichment.Interfaces
{
    public interface IFileConverterUtility
    {
        Task<bool> ConvertToMarkdown(
            string filePath,
            bool writeConvertedOutput,
            CancellationToken cancellationToken
        );

        Task<bool> ChunkExistingMarkdown(
            string filePath,
            CancellationToken cancellationToken
        );

        Task ClearOriginalFile(string filePath);
    }
}