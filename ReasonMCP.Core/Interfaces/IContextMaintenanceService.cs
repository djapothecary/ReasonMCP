using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.DTOs;
using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IContextMaintenanceService
    {
        Task<List<ChatMessageRecord>> SummarizeHistory(
            VSCodeChatPayloadDto payload
        );
    }
}