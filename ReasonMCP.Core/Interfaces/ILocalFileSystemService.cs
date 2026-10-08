using ReasonMCP.Core.Records;

namespace ReasonMCP.Core.Interfaces
{
    public interface ILocalFileSystemService
    {
        Task<List<FileAttachmentRecord>> GenerateDirectoryListAsync(
            string agentId,
            string absolutePath,
            CancellationToken cancellationToken = default
        );

        Task<string> WriteFileAsync(
            string agentId,
            string absolutePath,
            string content,
            CancellationToken cancellationToken = default
        );
    }
}