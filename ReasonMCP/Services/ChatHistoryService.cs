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
        private readonly Kernel _kernel;
        private readonly ChatHistoryService _chatHistoryService;
        private readonly ChatSettings _settings;
        private readonly ILogger<ChatHistoryService> _logger;

        public ChatHistoryService(
            Kernel kernel,
            ChatHistoryService chatHistoryService,
            IOptions<ChatSettings> options,
            ILogger<ChatHistoryService> logger
        )
        {
            _kernel = kernel;
            _chatHistoryService = chatHistoryService;
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

        public async Task<List<ChatMessageRecord>> LoadChatHistoryByAgentAsync(
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
    }
}