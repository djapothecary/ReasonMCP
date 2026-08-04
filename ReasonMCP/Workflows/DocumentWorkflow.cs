using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Interfaces.IEnrichment;

namespace ReasonMCP.Workflows
{
    public class DocumentWorkflow
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DocumentScanSettings _settings;
        private readonly StorageConfigSettings _storageSettings;
        private readonly ILogger<DocumentWorkflow> _logger;

        public DocumentWorkflow(
            IServiceScopeFactory scopeFactory,
            IOptions<DocumentScanSettings> options,
            IOptions<StorageConfigSettings> storageOptions,
            ILogger<DocumentWorkflow> logger
        )
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _storageSettings = storageOptions.Value;
            _logger = logger;
        }

        public async Task RunAsync(
            CancellationToken cancellationToken = default
        )
        {

        }
    }
}