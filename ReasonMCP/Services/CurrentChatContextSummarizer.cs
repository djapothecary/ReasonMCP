using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ReasonMCP.Services
{
    public class CurrentChatContextSummarizer
    {
        private readonly IChatCompletionService _chatService;
        private readonly ILogger<CurrentChatContextSummarizer> _logger;

        public CurrentChatContextSummarizer
        (
            [FromKeyedServices("MnemosyneService")] IChatCompletionService chatService,
            ILogger<CurrentChatContextSummarizer> logger
        )
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger;
        }

        public async Task<ChatHistory> SummarizeCurrentChatContext
        (
            ChatMessageContent systemPrompt,
            ChatHistory currentChatContext,
            CancellationToken cancellationToken = default
        )
        {

            string currentContextText = string.Join("\n", currentChatContext.Select(m => $"{m.Role}: {m.Content}"));

            string prompt = "Briefly summarize the following conversation into one concise paragraph. Focus on key FACTS, DECISIONS and PROBLEMS that are being solved. Do not lose the topic of the discussion.";

            var summaryChatContext = new ChatHistory();
            summaryChatContext.AddUserMessage($"{prompt}\n\n### CONVERSATION TO SUMMARIZE:\n{currentContextText}");

            var summaryResponse = await _chatService.GetChatMessageContentAsync(
                summaryChatContext,
                cancellationToken: cancellationToken
            );

            string summaryText = $"[SUMMARY OF PREVIOUS CONTEXT]: {summaryResponse.Content}";

            var newChatContext = new ChatHistory { systemPrompt };
            newChatContext.AddAssistantMessage(summaryText);

            //  add the last user prompt
            var lastUserMessage = currentChatContext.Last(m => m.Role == AuthorRole.User);
            newChatContext.AddUserMessage(lastUserMessage.Content!);

            return newChatContext;
        }
    }
}