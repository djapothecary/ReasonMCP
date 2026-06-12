using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.DTOs;

namespace ReasonMCP.Interfaces
{
    public interface IMnemosyne
    {
        Task<ChatHistory> CreateSummary(
            VSCodeChatPayloadDto payload,
            ChatHistory currentChatContext
        );

        Task WriteSummary(
            VSCodeChatPayloadDto payload,
            ChatHistory summaryHistory
        );
    }
}