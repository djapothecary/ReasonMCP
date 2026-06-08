using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ReasonMCP.Configurations;
using ReasonMCP.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ReasonMCP.Services
{
    public class AgentProfileService : IAgentProfileService
    {
        //  The cache: key is the filepath, Value is the parse profile
        private readonly ConcurrentDictionary<string, AgentProfile> _profileCache = new();
        private readonly ILogger<AgentProfileService> _logger;
        private readonly IDeserializer _deserialzer;

        public AgentProfileService
        (
            ILogger<AgentProfileService> logger
        )
        {
            _logger = logger;
            _deserialzer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()    // Prevents crashes if a new YAML field is added but the C# class isn't updated
                .Build();
        }

        public async Task<AgentProfile> LoadAgentProfileAsync(string filePath)
        {
            //  1.  Check Cache first! (O(1) memory lookup, zero disk I/O)
            if (_profileCache.TryGetValue(filePath, out var cachedProfile))
                return cachedProfile;


            //  2.  Fallback to Disk if not in the cache
            if (!File.Exists(filePath))
            {
                _logger.LogError("Agent profile YAML was not found at {filePath}", filePath);
                throw new FileNotFoundException($"Missing agent profile: {filePath}");
            }

            var yamlContent = await File.ReadAllTextAsync(filePath);
            var profile = _deserialzer.Deserialize<AgentProfile>(yamlContent);

            //  3.  Save to cache for the next time
            _profileCache.TryAdd(filePath, profile);

            return profile;
        }
    }
}