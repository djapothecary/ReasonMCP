using Microsoft.SemanticKernel;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Services
{
    public class ChatCompletionService : IReasonChatCompletionService
    {
        public Task<ChatMessageContent> GetResonChatAsync(string request)
        {
            throw new NotImplementedException();
        }
    }
}