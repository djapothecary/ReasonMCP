using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using ReasonMCP.Mappings;
using ReasonMCP.Records;

namespace ReasonMCP.Services
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly ChatSettings _settings;
        private readonly ILogger<ChatHistoryService> _logger;

        public ChatHistoryService(
            IOptions<ChatSettings> options,
            ILogger<ChatHistoryService> logger
        )
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<List<ChatMessageRecord>> LoadCurrentChatContextByAgentAsync(
            string fullPath
        )
        {
            if (!File.Exists(fullPath))
                return [];

            var json = await File.ReadAllTextAsync(fullPath);
            var chatHistory = JsonSerializer.Deserialize<ChatHistory>(json);

            //  Bridge ChatHistory to Reason ChatMessage using extension
            return chatHistory?.ToReasonChatMessage() ?? [];
        }

        /// <summary>
        /// Load the Chat History File from disk
        /// The "agent" is determined by the file path that is provided
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public async Task<List<ChatMessageRecord>> LoadChatHistoryFromFileByAgentAsync(
            string fullPath
        )
        {
            if (!File.Exists(fullPath))
                return [];

            var json = await File.ReadAllTextAsync(fullPath);
            var chatHistory = JsonSerializer.Deserialize<ChatHistory>(json);

            //  Bridge ChatHistory to Reason ChatMessage using extension
            return chatHistory?.ToReasonChatMessage() ?? [];
        }

        public async Task SaveCurrentChatContextByAgentAsync(
            ChatMessageRecord agentHistory,
            string fullPath
        )
        {
            if (!File.Exists(fullPath))
                return;

            List<ChatMessageRecord> history = [];

            if (File.Exists(fullPath))
            {
                var json = await File.ReadAllTextAsync(fullPath);
                history = JsonSerializer.Deserialize<List<ChatMessageRecord>>(json) ?? [];
            }

            history.Add(agentHistory);
            await File.WriteAllTextAsync(fullPath, JsonSerializer.Serialize(
                history,
                new JsonSerializerOptions { WriteIndented = true }
            ));

        }

        public async Task SaveHistoryByAgentAsync(
            ChatMessageRecord agentHistory,
            string fullPath
        )
        {
            if (!File.Exists(fullPath))
                return;

            List<ChatMessageRecord> history = [];

            if (File.Exists(fullPath))
            {
                var json = await File.ReadAllTextAsync(fullPath);
                history = JsonSerializer.Deserialize<List<ChatMessageRecord>>(json) ?? [];
            }

            history.Add(agentHistory);
            await File.WriteAllTextAsync(fullPath, JsonSerializer.Serialize(
                history,
                new JsonSerializerOptions { WriteIndented = true }
            ));

        }

        /// <summary>
        /// Appends the latest message to the chat history file
        /// Used for both "master" history and "current context"
        /// Agent and type ("master" or "current context") are
        /// determined by fullPath
        /// </summary>
        /// <param name="agentHistory"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public async Task AppendToHistoryFileAsync(
            ChatMessageRecord agentHistory,
            string fullPath
        )
        {
            //  1.  Serialize ONLY the single new message into a flat string
            var jsonLine = JsonSerializer.Serialize(agentHistory, new JsonSerializerOptions
            {
                WriteIndented = false   //  MUST be false for JSONL so it stays on one line
            });

            //  2.  Create the directory if it doesn't exist
            var fileInfo = new FileInfo(fullPath);
            fileInfo.Directory?.Create();

            //  3.  Append it directly to the end of the file.
            //  If the file doesn't exist, this natively creates it.
            //  This takes 1 millisecond and requires nearly zero RAM
            await File.AppendAllTextAsync(fullPath, jsonLine + Environment.NewLine);
        }
    }
}