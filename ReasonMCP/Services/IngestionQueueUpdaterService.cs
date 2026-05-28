using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;
using ReasonMCP.Orchestration;

namespace ReasonMCP.Services
{
    public class IngestionQueueUpdaterService : IIngestionQueueUpdaterService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly ILogger<IngestionQueueUpdaterService> _logger;

        public IngestionQueueUpdaterService(
            IServiceScopeFactory scopeFactory,
            IIngestionQueueService ingestionQueue,
            ILogger<IngestionQueueUpdaterService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _ingestionQueue = ingestionQueue;
            _logger = logger;
        }

        public async Task<bool> MarkConversionStatus(
            string filePath,
            bool chunkUpsertSuccess,
            CancellationToken cancellationToken
        )
        {
            try
            {
                if (chunkUpsertSuccess)
                {
                    await _ingestionQueue.MarkConversionCompleteAsync(filePath, cancellationToken);
                }
                else
                {
                    await _ingestionQueue.MarkConversionFailedAsync(filePath, "Conversion failed!", cancellationToken);
                }
            }
            catch (Exception conversionUpsertEx)
            {
                await _ingestionQueue.MarkFailedExceptionAsync(filePath, conversionUpsertEx.Message, cancellationToken);
            }

            return chunkUpsertSuccess;
        }
    }
}