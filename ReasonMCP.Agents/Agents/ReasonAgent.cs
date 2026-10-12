using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ReasonMCP.Agents.Tools;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Core.Interfaces;
using ReasonMCP.Core.Records;
using ReasonMCP.Core.Utilities;
using ReasonMCP.Skills.Interfaces;
using ReasonMCP.Skills.Tools;

namespace ReasonMCP.Agents.Agents
{
    public class ReasonAgent
    {
        private readonly Channel<dynamic> _agentTaskChannel;
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ChatSettings _settings;
        private readonly FileSystemSecuritySettings _securitySettings;
        private readonly ILogger<ReasonAgent> _logger;

        public ReasonAgent
        (
            Channel<dynamic> agentTaskChannel,
            Kernel kernel,
            IServiceProvider serviceProvider,
            [FromKeyedServices("Reason")] IChatCompletionService chatCompletionService,
            IOptionsMonitor<ChatSettings> options,
            IOptionsMonitor<FileSystemSecuritySettings> securitySettings,
            ILogger<ReasonAgent> logger
        )
        {
            _agentTaskChannel = agentTaskChannel;
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
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
            var reasonAgent = _settings.Agents["reason"];

            var agentResponseChatMessageRecord = new List<ChatMessageRecord>();
            try
            {
                //  1.  Securely load the exact toolbelt authorized by the YAML
                ConfigureAgentTools(agentProfile);

                var executionHistory = new ChatHistory();

                executionHistory.AddSystemMessage(agentProfile.SystemPrompt);
                executionHistory.AddRange(currentContext);
                executionHistory.AddUserMessage(prompt);

                var executionSettings = new OllamaPromptExecutionSettings
                {
                    Temperature = agentProfile.ExecutionSettings.Temperature,
                    TopP = agentProfile.ExecutionSettings.TopP,
                    FunctionChoiceBehavior = agentProfile.Permissions.AllowToolCalling
                        ? FunctionChoiceBehavior.Auto()
                        : FunctionChoiceBehavior.None(),
                    ServiceId = agentProfile.ExecutionSettings.ServiceId,
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };

                // var jsonDebug = System.Text.Json.JsonSerializer.Serialize(executionHistory);
                // Console.WriteLine(jsonDebug);

                var agentResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    executionHistory,
                    executionSettings,
                    _kernel
                );

                var cleandedAgentResponse = ProcessJsonResponseUtility.TryParseChatMessageContent(agentResponse);

                agentResponseChatMessageRecord.Add(
                    new ChatMessageRecord("assistant",
                    cleandedAgentResponse
                    ?? "Reason took SnowCrash.  Friend's don't let friends do SnowCrash ..."));

                var turnCount = currentContext.Count(m => m.Role == AuthorRole.User);
                if (turnCount > _settings.ChatSummarizationThreshold)
                {
                    _agentTaskChannel.Writer.TryWrite(new AgentTask(
                        "GradeResponse",
                        currentContextFilePath
                    ));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return agentResponseChatMessageRecord;
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

            //  1.  The Write File Tool (Requires the dynamic agent ID injection)
            //  CRITICAL: We must check if the plugin/tool is already loaded because _kernel
            //  is injected via the constructor and persists across chat turns!
            if (allowedTools.Contains("WriteFileTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("WriteFileTool"))
            {
                var fileSystem = _serviceProvider
                    .GetRequiredService<ILocalFileSystemService>();

                var writeFileTool = new WriteFileTool(
                    fileSystem,
                    profile.Name.ToLower()
                );

                _kernel.Plugins.AddFromObject(writeFileTool, "WriteFileTool");
            }

            if (allowedTools.Contains("LocalFileSystemTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("LocalFileSystemTool"))
            {
                var localFileSystem = _serviceProvider
                    .GetRequiredService<ILocalFileSystemService>();

                var localFileSystemTool = new LocalFileSystemTool(
                    localFileSystem,
                    profile.Name.ToLower(),
                    _securitySettings
                );

                _kernel.Plugins.AddFromObject(localFileSystemTool, "LocalFileSystemTool");
            }

            //  2.  The Microsoft Skills Tool
            if (allowedTools.Contains("MicrosoftSkillsTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("MicrosoftSkillsTool"))
            {
                var skillService = _serviceProvider
                    .GetRequiredService<ISkillLibraryService>();

                var skillsTool = new MicrosoftSkillsTools(skillService);

                _kernel.Plugins.AddFromObject(skillsTool, "MicrosoftSkillsTool");
            }

            //  3.  The Codebase Search Tool (Standard Injection)
            if (allowedTools.Contains("CodebaseSearchTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("CodebaseSearchTool"))
            {
                var codebaseTool = _serviceProvider
                    .GetRequiredService<CodebaseContextSearchTool>();

                _kernel.Plugins.AddFromObject(codebaseTool, "CodebaseSearchTool");
            }

            //  4.  The Documents Search Tool (Standard Injection)
            if (allowedTools.Contains("DocumentSearchTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("DocumentSearchTool"))
            {
                var documentsTool = _serviceProvider
                    .GetRequiredService<DocumentContextSearchTool>();

                _kernel.Plugins.AddFromObject(documentsTool, "DocumentSearchTool");
            }

            //  5.  The Reference Search Tool (Standard Injection)
            if (allowedTools.Contains("ReferenceSearchTool", StringComparer.OrdinalIgnoreCase) &&
                _kernel.Plugins.Contains("ReferenceSearchTool"))
            {
                var referencesTool = _serviceProvider
                    .GetRequiredService<ReferenceContextSearchTool>();

                _kernel.Plugins.AddFromObject(referencesTool, "ReferenceSearchTool");
            }

            //  6.  Random number Tool (Standard Injection)
            if (allowedTools.Contains("RandomNumberTool", StringComparer.OrdinalIgnoreCase) &&
                _kernel.Plugins.Contains("RandomNumberTool"))
            {
                var randomNumberTool = _serviceProvider
                    .GetRequiredService<RandomNumberTools>();

                _kernel.Plugins.AddFromObject(randomNumberTool, "RandomNumberTool");
            }
        }
    }
}