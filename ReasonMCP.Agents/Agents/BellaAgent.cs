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
    public class BellaAgent
    {
        private readonly Channel<dynamic> _agentTaskChannel;
        private readonly Kernel _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ChatSettings _settings;
        private readonly FileSystemSecuritySettings _securitySettings;
        private readonly ILogger<BellaAgent> _logger;

        public BellaAgent
        (
            Channel<dynamic> agentTaskChannel,
            Kernel kernel,
            IServiceProvider serviceProvider,
            [FromKeyedServices("Reason")] IChatCompletionService chatCompletionService,
            IOptionsMonitor<ChatSettings> options,
            IOptionsMonitor<FileSystemSecuritySettings> securitySettings,
            ILogger<BellaAgent> logger
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
        /// to the Bella Agent
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
            var agentResponseChatMessageRecord = new List<ChatMessageRecord>();
            try
            {
                var bellaAgent = _settings.Agents["bella"];

                //  1.  Get available tools
                ConfigureAgentTools(agentProfile);

                currentContext.AddUserMessage(prompt);
                currentContext.AddSystemMessage(agentProfile.SystemPrompt);

                var executionSettings = new OllamaPromptExecutionSettings
                {
                    Temperature = agentProfile.ExecutionSettings.Temperature,
                    TopP = agentProfile.ExecutionSettings.TopP,
                    FunctionChoiceBehavior = agentProfile.Permissions.AllowToolCalling
                        ? FunctionChoiceBehavior.Auto()
                        : FunctionChoiceBehavior.None(),
                    ServiceId = agentProfile.ExecutionSettings.ServiceId,   //  This refers to the named connection "Alpaca" or "Vicuna" (used for grading)
                    ExtensionData = new Dictionary<string, object> { { "raw", true } }
                };


                var agentResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    currentContext,
                    executionSettings,
                    _kernel
                );

                var cleandedAgentResponse = ProcessJsonResponseUtility.TryParseChatMessageContent(agentResponse);

                agentResponseChatMessageRecord.Add(
                    new ChatMessageRecord("assistant",
                        cleandedAgentResponse
                        ?? "Bella's chasing squirrels ..."));

                var turnCount = currentContext.Count(m => m.Role == AuthorRole.User);
                if (turnCount > _settings.ChatSummarizationThreshold)
                {
                    _agentTaskChannel.Writer.TryWrite(new AgentTask(
                        "PlayFetch",
                        currentContextFilePath
                    ));
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return agentResponseChatMessageRecord;
        }

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

            if (allowedTools.Contains("LocalFileTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("LocalFileTool"))
            {
                var localFileSystem = _serviceProvider
                    .GetRequiredService<ILocalFileSystemService>();

                var localFileSystemTool = new LocalFileSystemTool(
                    localFileSystem,
                    profile.Name.ToLower(),
                    _securitySettings
                );

                _kernel.Plugins.AddFromObject(localFileSystemTool, "LocalFileTool");
            }

            //  2.  The Microsoft Skills Tool
            if (allowedTools.Contains("MicrosoftSkillsTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("MicrosoftSkillsTool"))
            {
                var skillService = _serviceProvider
                    .GetRequiredService<ISkillLibraryService>();

                var skillsTool = new MicrosoftSkillsTools(skillService);

                _kernel.Plugins.AddFromObject(skillsTool, "MicrosoftSkillTool");
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
            if (allowedTools.Contains("DocumentsSearchTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("DocumentSearchTool"))
            {
                var documentsTool = _serviceProvider
                    .GetRequiredService<DocumentContextSearchTool>();

                _kernel.Plugins.AddFromObject(documentsTool, "DocumentSearchTool");
            }

            //  5.  The Reference Search Tool (standard Injection)
            if (allowedTools.Contains("ReferenceSearchTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("ReferenceSearchTool"))
            {
                var referencesTool = _serviceProvider
                    .GetRequiredService<ReferenceContextSearchTool>();

                _kernel.Plugins.AddFromObject(referencesTool, "ReferenceSearchTool");
            }

            //  6.  Random Number Tool (Standard Injection)
            if (allowedTools.Contains("RandomNumberTool", StringComparer.OrdinalIgnoreCase) &&
                !_kernel.Plugins.Contains("RandomNumberTool"))
            {
                var randomNumberTool = _serviceProvider
                    .GetRequiredService<RandomNumberTools>();

                _kernel.Plugins.AddFromObject(randomNumberTool, "RandomNumberTool");
            }
        }
    }
}