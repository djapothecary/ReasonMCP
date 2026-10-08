namespace ReasonMCP.Core.Interfaces
{
    public interface IFileSystemSecurityService
    {
        bool IsPathAllowed(string requestedPath);

        Task<bool> IsAgentAllowedForWriteHereAsync(
            string agentId,
            string absolutePath
        );
    }
}