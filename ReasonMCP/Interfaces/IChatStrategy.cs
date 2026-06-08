using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IChatStrategy
    {
        bool GetAgentStrategy(string agent);
        bool ShouldSummarize(int turnCount);
        string GenerateCurrentContextFilePath();
        string GetMasterHistoryFilePath();
        Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync();
        Task<List<ChatMessageRecord>> LoadChatHistoryAsync();
        Task<List<ChatMessageRecord>> LoadChatHistoryFromFileAsync();
        Task AppendToChathistory(ChatMessageRecord record);
        Task AppendToCurrentContext(ChatMessageRecord record);
        Task<ChatHistory> GetSummary(ChatHistory currentContext);
        Task<List<ChatMessageRecord>> RunAgent(
            ChatHistory currentContext,
            string prompt
        );
    }
}