using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Models;

namespace ReasonMCP.Core.Services
{
    public class AgenticPlaybookService : IAgenticPlaybookService
    {
        private readonly IOptions<PlaybookSettings> _playbookSettings;
        private readonly ILogger<AgenticPlaybookService> _logger;

        public AgenticPlaybookService(
            IOptions<PlaybookSettings> options,
            ILogger<AgenticPlaybookService> logger
        )
        {
            _playbookSettings = options;
            _logger = logger;
        }

        /// <summary>
        /// Parse the provided playbook filke path and serialze it
        /// to an AgenticPlaybook Model
        /// </summary>
        /// <param name="playbookFilepath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AgenticPlaybook> ParsePlaybookFileAsync(
            string playbookFilePath,
            CancellationToken cancellationToken = default
        )
        {
            //  Bail out if the file doesn't exist
            if (!File.Exists(playbookFilePath))
                return new();

            //  1.  Get the file contents
            var fileContent = await File.ReadAllTextAsync(playbookFilePath);

            //  2.Extract just the XML block
            var match = Regex.Match(fileContent, @"<reason_playbook>(.*?)</reason_playbook>", RegexOptions.Singleline);

            if (!match.Success)
            {
                throw new InvalidOperationException("No valid <reason_playbook> XML block found in the Markdown file.");
            }

            string xmlString = match.Value;

            //  2. Deserialize to the C# model
            var serializer = new XmlSerializer(typeof(AgenticPlaybook));
            using var reader = new StringReader(xmlString);

            var playbook = serializer.Deserialize(reader) as AgenticPlaybook ?? new AgenticPlaybook();

            return playbook;
        }

        public async Task<int> GetLastCompletedStepAsync(
            string sessionId,
            CancellationToken cancellationToken = default
        )
        {
            var fileName = sessionId + ".playbook" + _playbookSettings.Value.FileExtension;
            var fullPath = _playbookSettings.Value.RootDirectory +
                _playbookSettings.Value.PlaybookHistoryDirectory +
                "\\" + fileName;

            if (!File.Exists(fullPath))
                return 0;

            var json = await File.ReadAllTextAsync(
                fullPath,
                cancellationToken
            );

            var currentPlaybook = JsonSerializer.Deserialize<AgenticPlaybook>(json);

            return currentPlaybook!.LastStepCompleted;
        }

        public async Task SaveCheckpointAsync(
            AgenticPlaybook playbook,
            string sessionId,
            int lastStepCompleted,
            string playbookState,
            CancellationToken cancellationToken = default
        )
        {
            var fileName = sessionId + ".playbook" + _playbookSettings.Value.FileExtension;
            var fullPath = _playbookSettings.Value.RootDirectory +
                _playbookSettings.Value.PlaybookHistoryDirectory +
                "\\" + fileName;

            var updatedPlaybook = new AgenticPlaybook();

            if (File.Exists(fullPath))
            {
                var json = await File.ReadAllTextAsync(
                    fullPath,
                    cancellationToken
                );

                updatedPlaybook = JsonSerializer.Deserialize<AgenticPlaybook>(json);
            }

            //  update state
            updatedPlaybook!.Steps = playbook.Steps;
            updatedPlaybook!.LastStepCompleted = lastStepCompleted;
            updatedPlaybook!.PlaybookState = playbookState;

            //  Create the directory if it doesn't exist
            var fileInfo = new FileInfo(fullPath);
            fileInfo.Directory?.Create();

            await File.WriteAllTextAsync(
                fullPath,
                JsonSerializer.Serialize(
                    updatedPlaybook,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                )
            );
        }
    }
}