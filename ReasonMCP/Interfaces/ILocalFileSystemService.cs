namespace ReasonMCP.Interfaces
{
    public interface ILocalFileSystemService
    {
        Task<string> GenerateDirectoryListAsync(
            string absolutePath,
            CancellationToken cancellationToken
        );
    }
}