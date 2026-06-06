using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IChatStrategy
    {
        bool GetAgentStrategy(string agent);
        bool ShouldSummarize(int turnCount);
        Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync();
        Task<List<ChatMessageRecord>> LoadChatHistoryAsync();
        Task<List<ChatMessageRecord>> LoadChatHistoryFromFileAsync();
        Task SaveCurrentChatContextAsync();
        Task SaveChatHistoryAsync();
        Task<ChatHistory> GetSummary(ChatHistory currentContext);
        Task<List<ChatMessageRecord>> RunAgent();
    }
}