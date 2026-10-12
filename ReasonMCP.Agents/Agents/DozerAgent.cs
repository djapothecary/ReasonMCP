using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ReasonMCP.Agents.Tools;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Records;

namespace ReasonMCP.Agents.Agents
{
    /// <summary>
    /// This agent is responsible for executing the test suite.
    /// Dozer (The Mechanic) takes the compiled code, runs the test suite, and ensures the ship is structurally sound.
    /// </summary>
    public class DozerAgent
    {
        private readonly Channel<dynamic> _agentTaskChannel;
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ChatSettings _settings;
        private readonly FileSystemSecuritySettings _securitySettings;
        private readonly ILogger<DozerAgent> _logger;

        public DozerAgent
        (
            Channel<dynamic> agentTaskChannel,
            Kernel kernel,
            IServiceProvider serviceProvider,
            IOptionsMonitor<ChatSettings> options,
            IOptionsMonitor<FileSystemSecuritySettings> securitySettings,
            ILogger<DozerAgent> logger
        )
        {
            _agentTaskChannel = agentTaskChannel;
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _settings = options.CurrentValue;
            _securitySettings = securitySettings.CurrentValue;
            _logger = logger;
        }

        /// <summary>
        /// Sends the completed prompt (and attachments if there are any)
        /// to the Reason Agent
        /// </summary>
        /// <param name="agentProfile"></param>
        /// <param name="currentContext"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public async Task<List<ChatMessageRecord>> SendPrompt(
            AgentProfile agentProfile,
            ChatHistory currentContext,
            string prompt,
            string currentContextFilePath
        )
        {
            var dozzerAgent = _settings.Agents["dozer"];

            try
            {
                ConfigureAgentTools(agentProfile);
            }
            catch (System.Exception)
            {

                throw;
            }

            return [];
        }

        /// <summary>
        /// Configures all available tools for the agent
        /// </summary>
        /// <param name="profile"></param>
        private void ConfigureAgentTools(AgentProfile profile)
        {
            //  If the YAML explicitly disables tool calling entirely, bail out immediately
            if (!profile.Permissions.AllowToolCalling || profile.Permissions.AllowedTools == null)
                return;

            var allowedTools = profile.Permissions.AllowedTools;

            //  1.  Run Xunit Tests (standard Injection)
            if (allowedTools.Contains("RunUnitTestsTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("RunUnitTestsTool"))
            {
                var runUnitTestsTool = _serviceProvider
                    .GetRequiredService<RunUnitTestsTool>();

                _kernel.Plugins.AddFromObject(runUnitTestsTool, "RunUnitTestsTool");
            }

            //  2.  Read test results (Standard Inject)
            if (allowedTools.Contains("ReadTestsResultsTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("ReadTestsResultsTool"))
            {
                var readTestResultsTool = _serviceProvider
                    .GetRequiredService<ReadTestResultsTool>();

                _kernel.Plugins.AddFromObject(readTestResultsTool, "ReadTestsResultsTool");
            }
        }
    }
}