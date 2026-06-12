using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.DTOs;
using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IChatStrategy
    {
        bool GetAgentStrategy(string agent);
        bool ShouldSummarize(int turnCount);
        string GetMasterHistoryFilePath();
        Task<List<ChatMessageRecord>> LoadCurrentChatContextAsync();
        Task<List<ChatMessageRecord>> LoadChatHistoryAsync();
        Task<List<ChatMessageRecord>> LoadChatHistoryFromFileAsync();
        Task AppendToChathistory(ChatMessageRecord record);
        Task AppendToCurrentContext(ChatMessageRecord record, VSCodeChatPayloadDto payload);
        Task<ChatHistory> GetSummary(ChatHistory currentContext);
        Task<List<ChatMessageRecord>> RunAgent(
            VSCodeChatPayloadDto payload,
            ChatHistory currentContext,
            string prompt
        );
    }
}