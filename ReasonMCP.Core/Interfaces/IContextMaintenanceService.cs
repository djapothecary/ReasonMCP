using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Core.Interfaces
{
    public interface IContextMaintenanceService
    {
        Task<List<ChatMessageRecord>> SummarizeHistory(
            VSCodeChatPayloadDto payload
        );
    }
}