using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ReasonMCP.Core.Enums;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Models;
using ResourceType = ReasonMCP.Core.Enums.ResourceType;

namespace ReasonMCP.Core.Services
{

    public class AgentPermissionEvaluator : IAgentPermissionEvaluator
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly ILogger<AgentPermissionEvaluator> _logger;

        public AgentPermissionEvaluator(
            IMemoryCache cache,
            ILogger<AgentPermissionEvaluator> logger
        )
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(
            string agentId,
            ResourceType resourceType,
            string resourceTarget,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                _logger.LogWarning("[RESTRICTED ACCESS]: {Agent} has requested access to a restriced sector or policy.", agentId);
                return false;
            }

            //  1.  Fetch compiled permissions for the agent
            //  Cached to save DB roundtrips
            var permissions = await GetCachedPermissionsForAgentAsync(
                agentId,
                cancellationToken
            );

            //  2.  Filter permissions relevant to the requested Resource Type (e.g., Plugin)
            var relevantPermissions = permissions
                .Where(p => p.ResourceType == resourceType)
                .ToList();

            //  3.  Check for an explicit DENY first (Security Best Practice)
            bool hasDeny = relevantPermissions
                .Where(p => p.Effect == AccessEffect.Deny)
                .Any(p => MatchesTarget(
                    p.ResourceTarget,
                    resourceTarget
                )
            );

            if (hasDeny)
            {
                _logger.LogWarning("[ACCESS VIOLATION]: {Agent} is denied access.", agentId);
                return false;
            }

            //  4.  Check for an explicit ALLOW
            bool hasAllow = relevantPermissions
                .Where(p => p.Effect == AccessEffect.Allow)
                .Any(p => MatchesTarget(
                    p.ResourceTarget,
                    resourceTarget
                )
            );

            return hasAllow;
        }

        /// <summary>
        /// Evaluates if a permission rule target matches the requested resource.
        /// Supports wildcards like "GitHubPlugin.*" or "*"
        /// </summary>
        private static bool MatchesTarget(
            string ruleTarget,
            string requestedTarget
        )
        {
            if (ruleTarget == "*")
                return true;

            if (ruleTarget.Equals(requestedTarget, StringComparison.OrdinalIgnoreCase))
                return true;

            //  Convert wildcard pattern to a valid Regex pattern
            //  e.g., "GitHubPlugin.*" becomes "GitHubPlugin\..*$"
            string regexPattern = "^" + Regex.Escape(ruleTarget).Replace("\\", ".") + "$";

            return Regex.IsMatch(requestedTarget, regexPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Retrieves permissions from the database via flattened roles and caches them.
        /// </summary>
        private async Task<List<AgentPermission>> GetCachedPermissionsForAgentAsync(
            string agentId,
            CancellationToken cancellationToken
        )
        {
            string cacheKey = $"agent_perms_{agentId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                //  TODO:   Feature:    Build out SQLite database
                var agentPermissions = new List<AgentPermission>();
                // Fetch the agent, include roles, and flatten permissions
                // Adjust this LINQ query to exactly match your EF configurations
                // var agentPermissions = await _dbContext.Agents
                //     .Where(a => a.Id == agentId && a.IsActive)
                //     .SelectMany(a => a.Roles)
                //     .Select(ar => ar.Role)
                //     .SelectMany(r => r.Permissions)
                //     .ToListAsync();

                return agentPermissions;
            }) ?? new List<AgentPermission>();
        }
    }
}