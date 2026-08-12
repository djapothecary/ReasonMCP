namespace ReasonMCP.Interfaces.IEnrichment
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