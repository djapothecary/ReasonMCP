namespace ReasonMCP.Enrichment.Interfaces
{
    public interface IDocumentScanService
    {
        Task ScanDocumentsAsync(
            CancellationToken cancellationToken = default
        );

        Task ProcessDirectoryRecursivelyAsync(
            DirectoryInfo directory,
            CancellationToken cancellationToken = default
        );
    }
}