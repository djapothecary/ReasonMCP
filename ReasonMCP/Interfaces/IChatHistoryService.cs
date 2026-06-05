using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IChatHistoryService
    {
        Task<List<ChatMessageRecord>> LoadCurrentChatContextByAgentAsync(
            string fullPath);
        Task<List<ChatMessageRecord>> LoadChatHistoryByAgentAsync(
            string fullPath);

        Task SaveCurrentChatContextByAgentAsync(
            ChatMessageRecord agentHistory,
            string fullPath);

        Task SaveHistoryByAgentAsync(
            ChatMessageRecord agentHistory,
            string fullPath);
    }
}