namespace ReasonMCP.Enrichment.Interfaces
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