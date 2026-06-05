using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IChatStrategy
    {
        bool GetAgentStrategy(string agent);
        bool ShouldSummarize(int turnCount);
        Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync();
        Task<List<ChatMessageRecord>> LoadChatHistoryAsync();
        Task SaveCurrentChatContextAsync();
        Task SaveChatHistoryAsync();
    }
}