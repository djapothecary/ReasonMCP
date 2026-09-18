using Microsoft.SemanticKernel;
using ReasonMCP.Core.Interfaces;

namespace ReasonMCP.Core.Services
{
    public class ChatCompletionService : IReasonChatCompletionService
    {
        public Task<ChatMessageContent> GetResonChatAsync(string request)
        {
            throw new NotImplementedException();
        }
    }
}