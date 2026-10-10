using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReasonMCP.Core.Configurations;
using ReasonMCP.Skills.Interfaces;

namespace ReasonMCP.Skills.Services
{
    public class LocalSkillLibraryService : ISkillLibraryService
    {
        private readonly string _skillsRootDirectory;
        private readonly ILogger<LocalSkillLibraryService> _logger;

        public LocalSkillLibraryService(
            IOptionsMonitor<SkillSettings> options,
            ILogger<LocalSkillLibraryService> logger
        )
        {
            _skillsRootDirectory = options.CurrentValue.MicrosoftSkillsDirectory;
            _logger = logger;
        }

        /// <summary>
        /// Get the available skill Categories
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Available Skill Category names</returns>
        public Task<string> GetAvailableCategoriesAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (!Directory.Exists(_skillsRootDirectory))
                return Task.FromResult("[ERROR]:  Skill library directory not found.");

            var directories = Directory.GetDirectories(_skillsRootDirectory);
            var categoryNames = directories.Select(d => Path.GetDirectoryName(d));

            return Task.FromResult($"Available Skill Categories:\n- {string.Join(
                "\n- ", categoryNames)}\n\nUse the 'GetSkillsInCategory' tool to see the specific skills for one of these categories."
            );
        }

        /// <summary>
        /// Get the specific skills available in the provided category
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The summary of the skills and the skills themselves</returns>
        public async Task<string> GetSkillsInCategoryAsync(
            string categoryName,
            CancellationToken cancellationToken = default
        )
        {
            //  Sanitize input to prevent directory traversal attacks (Zero-Trust!)
            var safeCategory = Path.GetDirectoryName(categoryName);
            //  TODO:   This may through an error and need to me concatonated
            var categoryPath = Path.Combine(_skillsRootDirectory, safeCategory!);

            if (!Directory.Exists(categoryPath))
                return $"[ERROR]:  Category '{safeCategory}' does not exist.";

            var files = Directory.GetFiles(categoryPath, "*.md")
                .Where(f => !f.EndsWith("README.md",
                    StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileNameWithoutExtension(f));

            //  Optional:  Read the README.md to provide a summary of the category
            string summary = string.Empty;
            string readmePath = Path.Combine(categoryPath, "README.md");

            if (File.Exists(readmePath))
            {
                summary = $"Category Summary:\n{await File.ReadAllTextAsync(readmePath, cancellationToken)}\n\n";
            }

            return $"{summary}Available Skills in {safeCategory}:\n- {string.Join(
                "\n- ", files)}\n\nUse the 'LoadSpecificSkill' tool to inject one of these files into your context.";
        }

        /// <summary>
        /// Returns the content of the specific skill
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="skillName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetSpecificSkillContentAsync(
            string categoryName,
            string skillName,
            CancellationToken cancellationToken = default
        )
        {
            var safeCategory = Path.GetFileName(categoryName);
            var safeSkill = Path.GetFileNameWithoutExtension(safeCategory);

            var filePath = Path.Combine(
                _skillsRootDirectory,
                safeCategory,
                $"{safeSkill}.md"
            );

            if (!File.Exists(filePath))
                return $"[ERROR]:  Skill '{safeSkill}' not found in category '{safeCategory}'.";

            string content = await File.ReadAllTextAsync(
                filePath,
                cancellationToken
            );

            return $"### SKILL LOADED: {safeSkill}\n\n{content}";
        }
    }
}