using ReasonMCP.Configurations;

namespace ReasonMCP.Interfaces
{
    public interface IAgentProfileService
    {
        Task<AgentProfile> LoadAgentProfileAsync(string filePath);
    }
}