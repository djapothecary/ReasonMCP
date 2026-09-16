using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface ILocalFileSystemService
    {
        Task<List<FileAttachmentRecord>> GenerateDirectoryListAsync(
            string agentId,
            string absolutePath,
            CancellationToken cancellationToken
        );
    }
}