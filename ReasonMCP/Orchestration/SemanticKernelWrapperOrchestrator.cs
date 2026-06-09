using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Configurations;
using ReasonMCP.DTOs;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;
using ReasonMCP.Utilities;

namespace ReasonMCP.Orchestration
{
    public class SemanticKernelWrapperOrchestrator
    {
        private readonly IEnumerable<IChatStrategy> _strategies;
        private readonly ChatSettings _settings;
        private readonly ILogger<SemanticKernelWrapperOrchestrator> _logger;
        private readonly CancellationToken cancellationToken;

        public SemanticKernelWrapperOrchestrator
        (
            IEnumerable<IChatStrategy> strategies,
            IOptions<ChatSettings> options,
            ILogger<SemanticKernelWrapperOrchestrator> logger
        )
        {
            _strategies = strategies;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> ProcessChatAsync(
            VSCodeChatPayloadDto payload
        )
        {
            //  1.  Convert DTOs to SK ChatHistory
            var skChathistory = new ChatHistory();
            var currentChatContext = new ChatHistory();

            foreach (var turn in payload.History)
            {
                skChathistory.AddMessage(new AuthorRole(turn.Role), turn.Content);
            }

            //  2. Dynamic Persona routing
            string agentId = payload.AgentId.ToLower();
            var agentStrategy = _strategies.FirstOrDefault(s => s.GetAgentStrategy(agentId));

            //  update the prompt for file attachments
            //  if no files are attached the original prompt is returned
            var augmentedPrompt = payload.ToAugmentedPrompt();

            //  3.  Add current message to "master" chat history regardless
            await agentStrategy!.AppendToChathistory(new ChatMessageRecord("user", augmentedPrompt));

            //  TODO:   Refactor:   need to work out logic of creating new currentContext file
            // await agentStrategy!.AppendToCurrentContext(new ChatMessageRecord("user", augmentedPrompt));

            //  4. Determine if summary needed
            var turnCount = payload.History.Count(m => m.Role == "user");
            var shouldSummarize = agentStrategy!.ShouldSummarize(turnCount);

            if (shouldSummarize)
            {
                //  perform current chat context summarization
                currentChatContext = await agentStrategy!.GetSummary(skChathistory);
            }
            else
            {
                foreach (var message in payload.History)
                {
                    currentChatContext.Add(new ChatMessageContent(
                        new AuthorRole(message.Role),
                        message.Content
                    ));
                }
            }

            //  5.Call _kernel.InvokePromptAsync() or IChatCompletionService
            var agentResponse = await agentStrategy!.RunAgent(
                                currentChatContext,
                                augmentedPrompt);

            //  6.  Append agent response to to master history
            await agentStrategy!.AppendToChathistory(new ChatMessageRecord(
                                "assistant",
                                agentResponse.First().Content));

            //  4.  Return text

            return agentResponse.First().Content;
        }
    }
}