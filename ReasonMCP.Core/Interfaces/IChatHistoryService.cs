using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Core.Interfaces
{
    public interface IChatHistoryService
    {
        Task<List<ChatMessageRecord>> LoadCurrentChatContextByAgentAsync(
            string fullPath);

        /// <summary>
        /// Load the JSON history
        /// Agent is determined by file path
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        Task<List<ChatMessageRecord>> LoadChatHistoryFromFileByAgentAsync(
            string fullPath);

        Task SaveCurrentChatContextByAgentAsync(
            ChatMessageRecord agentHistory,
            string fullPath);

        Task SaveHistoryByAgentAsync(
            ChatMessageRecord agentHistory,
            string fullPath);

        Task AppendToHistoryFileAsync(
            ChatMessageRecord agentHistory,
            string fullPath
        );

        Task AppendToPromptHistoryFileAsync(
            VSCodeChatPayloadDto payload
        );
    }
}