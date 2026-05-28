using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Models;
using ReasonMCP.Orchestration;
using ReasonMCP.Tools;

namespace ReasonMCP.Workers
{
    public class CodebaseScanWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IIngestionQueueService _ingestionQueue;
        private readonly IEnumerable<IFileConverterStrategy> _strategies;
        private readonly CodebaseScanSettings _settings;
        private readonly ILogger<CodebaseScanWorker> _logger;

        public CodebaseScanWorker(
            IServiceScopeFactory scopeFactory,
            IIngestionQueueService ingestionQueue,
            IEnumerable<IFileConverterStrategy> strategies,
            IOptions<CodebaseScanSettings> options,
            ILogger<CodebaseScanWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _ingestionQueue = ingestionQueue;
            _strategies = strategies;
            _settings = options.Value;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (!_settings.Enabled)
                return;

            _logger.LogInformation("Codebase Scan Worker started ...");

            try
            {
                //  1.  Create a fresh scope for Codebase scanning
                using var scope = _scopeFactory.CreateScope();

                //  2.  Scan Codebase directories
                var codebaseOrchestrator = scope.ServiceProvider.GetRequiredService<CodebaseScanOrchestrator>();
                await codebaseOrchestrator.ScanCodebaseAsync(cancellationToken);

                _logger.LogTrace("Codebase scan complete. Sleeping ...");
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while scanning Codebase directories: {Ex}", ex);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var file = new FileIngestionRecord();
                if (file == null)
                {
                    await Task.Delay(5000, cancellationToken);
                    continue;
                }


                try
                {
                    bool conversionSuccesss;

                    //  1.  Get the next file to process by "TargetStore = "Codebase" "
                    file = await _ingestionQueue.DequeueNextFileAsync("Codebase", cancellationToken);

                    //  2.  Determine the file type and processor to use
                    var strategy = _strategies.FirstOrDefault(s => s.CanConvert(file!.FilePath));

                    conversionSuccesss = await strategy!
                                    .ConvertForIngestionAsync(file!.FilePath);

                    if (conversionSuccesss)
                    {
                        await _ingestionQueue.MarkConversionCompleteAsync(file!.FilePath!, cancellationToken);
                    }
                    else
                    {
                        await _ingestionQueue.MarkConversionFailedAsync(file!.FilePath, "Upsert failed", cancellationToken);
                    }
                }
                catch (Exception whileEx)
                {
                    _logger.LogError("An Error occurred during the Upsert for {File}", file!.FilePath);
                    await _ingestionQueue.MarkFailedExceptionAsync(file!.FilePath, whileEx.Message, cancellationToken);
                }
            }
        }
    }
}