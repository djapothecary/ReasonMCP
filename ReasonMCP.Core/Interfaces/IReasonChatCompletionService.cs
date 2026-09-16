using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ReasonMCP.Interfaces
{
    public interface IReasonChatCompletionService
    {
        /// <summary>
        /// Processes a user request by loading history, invoking the AI with RAG/Plugins,
        /// and persisting the updated conversation.
        /// </summary>
        /// <param name="request">The raw text input from the user.</param>
        /// <returns>The full ChatMessageContent from the AI.</returns>
        Task<ChatMessageContent> GetResonChatAsync(string request);
    }
}