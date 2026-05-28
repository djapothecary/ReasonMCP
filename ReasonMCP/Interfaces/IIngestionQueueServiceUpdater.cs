namespace ReasonMCP.Interfaces
{
    public interface IIngestionQueueUpdaterService
    {
        Task<bool> MarkConversionStatus(
            string filePath,
            bool chunkUpsertSuccess,
            CancellationToken cancellationToken
        );
    }
}