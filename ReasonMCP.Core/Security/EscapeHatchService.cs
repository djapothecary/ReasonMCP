using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Core.Configurations;

namespace ReasonMCP.Core.Security
{
    public class EscapeHatchService : BackgroundService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IOptionsMonitor<EscapeHatchSettings> _settings;
        private readonly ILogger<EscapeHatchService> _logger;

        public EscapeHatchService(
            IHostApplicationLifetime appLifeTime,
            IOptionsMonitor<EscapeHatchSettings> settings,
            ILogger<EscapeHatchService> logger
        )
        {
            _appLifetime = appLifeTime;
            _settings = settings;
            _logger = logger;
        }

        protected override Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            var escapeHatchPath = _settings.CurrentValue.EscapeHatchPath;

            //  the user has INTENIONALLY turned off the escape hatch
            //  THIS IS DANGEROUS
            if (!_settings.CurrentValue.Enabled)
                return Task.CompletedTask;

            if (!Directory.Exists(escapeHatchPath))
            {
                Directory.CreateDirectory(escapeHatchPath);
            }

            var watcher = new FileSystemWatcher(escapeHatchPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            //  The Event Listener:  Fires the millisecond a file is created
            watcher.Created += (sender, e) => TriggerNuclearShutdown(e.FullPath);

            return Task.CompletedTask;
        }

        private void TriggerNuclearShutdown(string filePath)
        {
            _logger.LogCritical("\n[SECURITY OVERRIDE] POISON PILL DETECTED: {file}. INITIATING IMMEDIATE SYSTEM SHUTDOWN.", filePath);

            // Optional: Delete the file so it doesn't instantly crash the app on the next reboot
            try { File.Delete(filePath); } catch { /* Ignore lock errors, we are dying anyway */ }

            // OPTION 1: Graceful Shutdown (Cancels all tokens, finishes current HTTP requests)
            _appLifetime.StopApplication();

            // OPTION 2: The EMP (Immediate, brutal OS-level process kill. Use this if you don't trust graceful shutdowns)
            // Environment.Exit(-1);
        }
    }
}