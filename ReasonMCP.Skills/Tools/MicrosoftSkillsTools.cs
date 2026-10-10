using System.ComponentModel;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;
using ReasonMCP.Skills.Interfaces;
using ReasonMCP.Skills.Services;

namespace ReasonMCP.Skills.Tools
{
    public class MicrosoftSkillsTools
    {
        private readonly ISkillLibraryService _skillLibraryService;

        public MicrosoftSkillsTools(
            ISkillLibraryService skillLibraryService
        )
        {
            _skillLibraryService = skillLibraryService;
        }

        [McpServerTool(Name = "explore_skill_categories")]
        [KernelFunction("explore_skill_categories")]
        [Description("Lists all high-level categories of available C# and .NET developer skills.")]
        public async Task<string> ExploreSkillCategoriesAsync(
            CancellationToken cancellationToken
        )
        {
            return await _skillLibraryService.GetAvailableCategoriesAsync(
                cancellationToken
            );
        }

        [McpServerTool(Name = "get_skills_in_category")]
        [KernelFunction("get_skills_in_category")]
        [Description("Lists the specific skill documents available inside a given category.")]
        public async Task<string> GetSkillsInCategoryAsync(
            [Description("The exact name of the category (e.g. 'dotnet-advanced)")] string categoryName,
            CancellationToken cancellationToken
        )
        {
            return await _skillLibraryService.GetSkillsInCategoryAsync(
                categoryName,
                cancellationToken
            );
        }

        [McpServerTool(Name = "load_specific_skill")]
        [KernelFunction("load_specific_skill")]
        [Description("Loads the exact technical instructions and best practices for a specific skill into your context window.")]
        public async Task<string> LoadSpecificSkillAsync(
            [Description("The category name")] string categoryName,
            [Description("The specific skill name (without the ,md extension)")] string skillName,
            CancellationToken cancellationToken
        )
        {
            return await _skillLibraryService.GetSpecificSkillContentAsync(
                categoryName,
                skillName,
                cancellationToken
            );
        }
    }
}