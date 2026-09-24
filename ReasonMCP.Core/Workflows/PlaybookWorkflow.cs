using System.Text;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Protocol;
using ReasonMCP.Core.DTOs;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Models;
using ReasonMCP.Core.Services;

namespace ReasonMCP.Core.Workflows
{
    public class PlaybookWorkflow : IPlaybookWorkflow
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PlaybookWorkflow> _logger;

        public PlaybookWorkflow(
            IServiceScopeFactory scopeFactory,
            ILogger<PlaybookWorkflow> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        public async Task<string> ProcessPlaybookWorkflowAsync(
            string playbookFilePath,
            string sessionId,
            string userCommand, // e.g., "start", "continue", "grade", "abort"
            CancellationToken cancellationToken
        )
        {
            //  Bail out if no playbook provided
            if (playbookFilePath == string.Empty)
                return "No Playbook was provided.";

            using var scope = _scopeFactory.CreateScope();

            var playbookService = scope
                .ServiceProvider
                .GetRequiredService<IAgenticPlaybookService>();

            var strategies = scope
                .ServiceProvider
                .GetRequiredService<IEnumerable<IChatStrategy>>();

            //  1.  Load and parse playbook to the model
            var playbook = await playbookService.ParsePlaybookFileAsync(
                playbookFilePath,
                cancellationToken
            );

            // log the playbook, sessionId and last completed step
            //  this will be 0 if this is the first attempt
            await playbookService.SaveCheckpointAsync(
                playbook,
                sessionId,
                0,
                "Started",
                cancellationToken
            );

            int currentStepIndex = await playbookService.GetLastCompletedStepAsync(
                sessionId,
                cancellationToken
            ) + 1;

            //  Handle "abort" or Completion
            if (userCommand.ToLower() == "abort")
                return "Playbook execution aborted by user.";

            if (currentStepIndex > playbook.Steps.Count)
                return "Playbook is already complete.";

            //  2.  Get the Current step
            var step = playbook.Steps.FirstOrDefault(
                s => s.StepNumber == currentStepIndex
            );

            if (step == null)
                return $"Error: Step {currentStepIndex} not found.";

            //  3.  Execute the Single Step
            var agentStrategy = strategies.FirstOrDefault(
                s => s.GetAgentStrategy(
                    step.TargetAgentId.ToLower()
                )
            );

            // --> Build your context and send the prompt to the specific agent here <--
            // var agentResponseContent = await agentStrategy.SendPrompt(...);
            var convertedStepPrompt = AssembleAgentPrompt(step);

            //  Build the step as a PayloadDto
            var stepPayload = new VSCodeChatPayloadDto
            {
                SessionId = sessionId,
                AgentId = step.TargetAgentId,
                Role = "user"
            };

            var promptChatHistory = new ChatHistory();

            var agentResponseContent = await agentStrategy!.RunAgent(
                stepPayload,
                promptChatHistory,
                convertedStepPrompt
            );

            //  4.  Chekpoint the state
            await playbookService.SaveCheckpointAsync(
                playbook,
                sessionId,
                currentStepIndex,
                $"Step {currentStepIndex} Completed",
                cancellationToken
            );

            //  5.  Determin the next UX action (The HITL pause)
            var sb = new StringBuilder();
            sb.AppendLine($"### Step {step.StepNumber}: {step.PromptDescription} - **COMPLETE**");
            sb.AppendLine($"**{step.TargetAgentId} Output:**");

            var actualResponseText = agentResponseContent.FirstOrDefault()?.Content ?? "No response generated.";
            sb.AppendLine(actualResponseText); // The text from the agent

            //  Look ahead to the next step to see if we need a HITL pause
            var nextStep = playbook.Steps.FirstOrDefault(
                s => s.StepNumber == currentStepIndex + 1
            );

            if (nextStep != null)
            {
                if (nextStep.RequiresHumanApproval)
                {
                    sb.AppendLine("---");
                    sb.AppendLine($"⚠️ **HUMAN AUTHORIZATION REQUIRED FOR STEP {nextStep.StepNumber}** ⚠️");
                    sb.AppendLine($"*Next Task: {nextStep.TaskToComplete}*");
                    sb.AppendLine("\nType **continue** to proceed, **grade** to invoke Seraph, or **abort** to cancel.");
                }
                else
                {
                    sb.AppendLine("---");
                    sb.AppendLine($"*Auto-proceeding is available. Type **continue** to execute Step {nextStep.StepNumber}.*");
                }
            }
            else
            {
                sb.AppendLine("\n🎉 **PLAYBOOK COMPLETE!**");
            }

            //  mark playbook completed
            playbook.PlaybookCompleted = true;
            return sb.ToString();
        }

        /// <summary>
        /// Translates the Orchestration Step into a useable LLM prompt for the Agent
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        private string AssembleAgentPrompt(
            OrchestrationStep step
        )
        {
            var sb = new StringBuilder();

            //  1.  The Core Persona / Instruction
            if (!string.IsNullOrWhiteSpace(step.AgentPrompt))
            {
                sb.AppendLine(step.AgentPrompt);
                sb.AppendLine();
            }

            //  2.  The Context
            sb.AppendLine("### Context & Goal");
            sb.AppendLine(step.PromptDescription);
            sb.AppendLine();

            //  3.  The Task
            sb.AppendLine("### Task to Complete");
            sb.AppendLine(step.TaskToComplete);
            sb.AppendLine();

            //  4.  The Constraints (Critical for the LLM's attention mechanism)
            if (!string.IsNullOrWhiteSpace(step.Constraints))
            {
                sb.AppendLine("### Constraints & Rules");
                sb.AppendLine(step.Constraints);
                sb.AppendLine();
            }

            //  5.  OutPut Requirements
            if (!string.IsNullOrWhiteSpace(step.OutputRequirements))
            {
                sb.AppendLine("### Output Requirements");
                sb.AppendLine(step.OutputRequirements);
            }

            return sb.ToString().Trim().Replace("\r\n", "\n");
        }
    }
}