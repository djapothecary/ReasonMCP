using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.DTOs;
using ReasonMCP.Interfaces;
using ReasonMCP.Mappings;
using ReasonMCP.Records;

namespace ReasonMCP.Services
{
    public class ContextMaintenanceService : IContextMaintenanceService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public ContextMaintenanceService(
            Kernel kernel,
            IChatCompletionService chatService
        )
        {
            _kernel = kernel;
            _chatService = chatService;
        }
        public async Task<List<ChatMessageRecord>> SummarizeHistory(
            VSCodeChatPayloadDto payload
        )
        {
            //  Map to Semantic Kernel Types
            var skHistory = new ChatHistory();
            foreach (var msg in payload.History)
            {
                skHistory.AddMessage(new AuthorRole(msg.Role), msg.Content);
            }

            //  2.  Perform AI summarization
            //  Identify the 'middle' and 'recent' messages,
            // skipping the System Prompt (1) and taking the last 10
            var messagesToSummarize = skHistory.Skip(1).Take(skHistory.Count - 11).ToList();

            //  3.  Create a temporary history and add the data
            var tempHistory = new ChatHistory("You are a technical conversation secretary.");

            //  4.  Format the history into a single string for the AI to read
            string historyText = string.Join("\n", messagesToSummarize
                                        .Select(m => $"{m.Role}: {m.Content}"));

            string prompt = "Briefly summarize the following history into one concise paragraph. " +
                            "Focus on key facts and the technical parameters.  Do not lose the scope of the discussion.";

            //  5.  Add the prompt and the history text to the temp history
            tempHistory.AddUserMessage($"{prompt}\n\n###CONVERSATION TO SUMMARIZE:\n{historyText}");

            //  6.  Setup CancellationToken just before it is needed
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            //  7.  Generate the Summary
            var summaryResponse = await _chatService.GetChatMessageContentAsync(
                tempHistory,
                kernel: _kernel,
                cancellationToken: cts.Token
            );

            string summaryText = $"[SUMMARY OF PREVIOUS CONVERSATION]: {summaryResponse.Content}";

            //  8.  Rebuild the history
            var systemPrompt = skHistory.FirstOrDefault(m => m.Role == AuthorRole.System);
            var recentMessages = skHistory.TakeLast(10).ToList();

            var result = new List<ChatMessageRecord>();

            //  Preserve the "North Star" (System Prompt)
            if (systemPrompt != null)
                result.Add(new ChatMessageRecord("system", systemPrompt.Content!));

            //  Inject the "condensed" summary
            result.Add(new ChatMessageRecord("assistant", summaryText));

            //  Append the recent context
            result.AddRange(new ChatHistory(recentMessages).ToReasonChatMessage());

            return result;
        }
    }
}