using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Core.DTOs;

namespace ReasonMCP.Core.Interfaces
{
    public interface IGladosAgent
    {
        Task<ChatHistory> CreatePlaybookAsync(
            VSCodeChatPayloadDto payload,
            CancellationToken cancellationToken = default
        );

        Task WritePlaybookAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory playbookHistory,
            CancellationToken cancellationToken = default
        );

        Task WritePlaybookHistoryAsync(
            VSCodeChatPayloadDto payload,
            ChatHistory playbookHistory,
            CancellationToken cancellationToken = default
        );
    }
}