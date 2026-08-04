namespace ReasonMCP.Interfaces.IEnrichment
{
    public interface IReferenceDataScanService
    {
        Task ScanReferenceDataAsync(
            CancellationToken cancellationToken = default
        );

        Task ProcessDirectoryRecursivelyAsync(
            DirectoryInfo directory,
            CancellationToken cancellationToken = default
        );
    }
}