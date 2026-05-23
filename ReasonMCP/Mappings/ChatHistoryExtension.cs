using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Records;

namespace ReasonMCP.Mappings
{
    public static class ChatHistoryExtensions
    {
        /// <summary>
        /// Converts the Semantic Kernel ChatHistory into a transport-ready list of RefineryMessages.
        /// </summary>
        public static List<ChatMessageRecord> ToReasonChatMessage(this ChatHistory history)
        {
            //  Intentionally not using simplified initialization for readability
            return history.Select(m => new ChatMessageRecord(
                m.Role.Label,
                m.Content ?? string.Empty)
            ).ToList();
        }

        /// <summary>
        /// Reconstitutes a Semantic Kernel ChatHistory from a list of RefineryMessages.
        /// </summary>
        public static ChatHistory ToSemanticKernelChatHistory(this IEnumerable<ChatMessageRecord> messages)
        {
            var history = new ChatHistory();
            foreach (var msg in messages)
            {
                history.AddMessage(
                    new AuthorRole(msg.Role),
                    msg.Content
                );
            }

            return history;
        }
    }
}