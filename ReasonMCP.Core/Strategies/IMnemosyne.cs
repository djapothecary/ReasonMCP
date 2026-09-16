using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Core.DTOs;

namespace ReasonMCP.Core.Interfaces
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