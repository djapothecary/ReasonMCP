using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Configurations;
using ReasonMCP.DTOs;
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

        public async Task<List<ChatMessageRecord>> LoadAgentHistoryAsync()
        {
            var rootDirectory = _settings.RootDirectory;
            var directory = _settings.AgentHistoryDirectory;
            var fileName = _settings.AgentHistoryFilename;

            //  Not using Path.Combine here as it will return only the last value
            var fullPath = rootDirectory + directory + fileName;

            if (!File.Exists(fullPath))
                return [];

            var json = await File.ReadAllTextAsync(fullPath);
            var chatHistory = JsonSerializer.Deserialize<ChatHistory>(json);

            //  Bridge ChatHistory to Reason ChatMessage using extension
            return chatHistory?.ToReasonChatMessage() ?? [];

        }

        public async Task<List<ChatMessageRecord>> LoadChatHistoryAsync()
        {
            var rootDirectory = _settings.RootDirectory;
            var directory = _settings.ChatHistoryDirectory;
            var fileName = _settings.ChatHistoryFilename;

            //  Not using Path.Combine here as it will return only the last value
            var fullPath = rootDirectory + directory + fileName;

            if (!File.Exists(fullPath))
                return [];

            var json = await File.ReadAllTextAsync(fullPath);
            var chatHistory = JsonSerializer.Deserialize<ChatHistory>(json);

            //  Bridge ChatHistory to Reason ChatMessage using extension
            return chatHistory?.ToReasonChatMessage() ?? [];
        }

        public async Task<List<ChatMessageRecord>> LoadPlanHistoryAsync()
        {
            var rootDirectory = _settings.RootDirectory;
            var directory = _settings.PlanHistoryDirectory;
            var fileName = _settings.PlanHistoryFilename;

            //  Not using Path.Combine here as it will return only the last value
            var fullPath = rootDirectory + directory + fileName;

            if (!File.Exists(fullPath))
                return [];

            var json = await File.ReadAllTextAsync(fullPath);
            var chatHistory = JsonSerializer.Deserialize<ChatHistory>(json);

            //  Bridge ChatHistory to Reason ChatMessage using extension
            return chatHistory?.ToReasonChatMessage() ?? [];
        }

        public async Task SaveAgentHistoryAsync(ChatMessageRecord agentHistory)
        {
            var rootDirectory = _settings.RootDirectory;
            var directory = _settings.AgentHistoryDirectory;
            var fileName = _settings.AgentHistoryFilename;

            //  Not using Path.Combine here as it will return only the last value
            var fullPath = rootDirectory + directory + fileName;

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

        public async Task SaveChatHistory(ChatMessageRecord chatHistory)
        {
            var rootDirectory = _settings.RootDirectory;
            var directory = _settings.AgentHistoryDirectory;
            var fileName = _settings.AgentHistoryFilename;

            //  Not using Path.Combine here as it will return only the last value
            var fullPath = rootDirectory + directory + fileName;

            List<ChatMessageRecord> history = [];

            if (File.Exists(fullPath))
            {
                var json = await File.ReadAllTextAsync(fullPath);
                history = JsonSerializer.Deserialize<List<ChatMessageRecord>>(json) ?? [];
            }

            history.Add(chatHistory);
            await File.WriteAllTextAsync(fullPath, JsonSerializer.Serialize(
                history,
                new JsonSerializerOptions { WriteIndented = true }
            ));
        }

        public async Task SavePlanHistoryAsync(ChatMessageRecord planHistory)
        {
            var rootDirectory = _settings.RootDirectory;
            var directory = _settings.AgentHistoryDirectory;
            var fileName = _settings.AgentHistoryFilename;

            //  Not using Path.Combine here as it will return only the last value
            var fullPath = rootDirectory + directory + fileName;

            List<ChatMessageRecord> history = [];

            if (File.Exists(fullPath))
            {
                var json = await File.ReadAllTextAsync(fullPath);
                history = JsonSerializer.Deserialize<List<ChatMessageRecord>>(json) ?? [];
            }

            history.Add(planHistory);
            await File.WriteAllTextAsync(fullPath, JsonSerializer.Serialize(
                history,
                new JsonSerializerOptions { WriteIndented = true }
            ));
        }
    }
}