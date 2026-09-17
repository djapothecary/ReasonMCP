using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Core.Interfaces
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
        Task<List<ChatMessageRecord>> RunAgent(
            VSCodeChatPayloadDto payload,
            ChatHistory currentContext,
            string prompt
        );
    }
}