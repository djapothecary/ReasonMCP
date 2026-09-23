using ReasonMCP.Core.Models;

namespace ReasonMCP.Core.Interfaces
{
    public interface IAgenticPlaybookService
    {
        Task<AgenticPlaybook> ParsePlaybookFileAsync(
            string playbookFilepath,
            CancellationToken cancellationToken = default
        );

        Task<int> GetLastCompletedStepAsync(
            string sessionId,
            CancellationToken cancellationToken = default
        );

        Task SaveCheckpointAsync(
            AgenticPlaybook playbook,
            string sessionId,
            int lastStepCompleted,
            string playbookState,
            CancellationToken cancellationToken = default
        );
    }
}