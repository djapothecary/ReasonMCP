namespace ReasonMCP.Interfaces
{
    public interface IFileConverterStrategy
    {
        bool CanConvert(string filePath);
        Task<bool> ConvertForIngestionAsync(
            string filePath,
            CancellationToken cancellationToken
            );
    }
}