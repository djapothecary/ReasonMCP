using Microsoft.Extensions.AI;
using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IChatHistoryService
    {
        Task<List<ChatMessageRecord>> LoadChatHistoryAsync();
        Task SaveChatHistory(ChatMessageRecord chatHistory);
        Task<List<ChatMessageRecord>> LoadAgentHistoryAsync();
        Task SaveAgentHistoryAsync(ChatMessageRecord agentHistory);
        Task<List<ChatMessageRecord>> LoadPlanHistoryAsync();
        Task SavePlanHistoryAsync(ChatMessageRecord planHistory);
    }
}