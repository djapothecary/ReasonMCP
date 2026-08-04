namespace ReasonMCP.Interfaces.IEnrichment
{
    public interface ICodebaseScanService
    {
        Task ScanCodebaseAsync(
            CancellationToken cancellationToken = default
        );

        Task ProcessDirectoryRecursivelyAsync(
            DirectoryInfo directory,
            CancellationToken cancellationToken = default
        );
    }
}