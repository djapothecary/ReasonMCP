namespace ReasonMCP.Core.Interfaces
{
    public interface IFileSystemSecurityService
    {
        bool IsPathAllowed(string requestedPath);
    }
}