using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Core.DTOs;

namespace ReasonMCP.Core.Interfaces
{
    public interface IMnemosyneAgent
    {
        Task<ChatHistory> CreateSummaryAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory currentChatContext,
            CancellationToken cancellationToken = default
        );

        Task WriteSummaryAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory summaryHistory,
            CancellationToken cancellationToken = default
        );
    }
}