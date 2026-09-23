namespace ReasonMCP.Core.Interfaces
{
    public interface IPlaybookWorkflow
    {
        Task<string> ProcessPlaybookWorkflowAsync(
            string playbookFilePath,
            string sessionId,
            string userCommand, // e.g., "start", "continue", "grade", "abort"
            CancellationToken cancellationToken
        );
    }
}