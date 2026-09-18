using ReasonMCP.Core.Configurations;

namespace ReasonMCP.Core.Interfaces
{
    public interface IAgentProfileService
    {
        Task<AgentProfile> LoadAgentProfileAsync(string filePath);
    }
}