namespace ReasonMCP.Interfaces
{
    public interface IFileSystemSecurityService
    {
        bool IsPathAllowed(string requestedPath);
    }
}