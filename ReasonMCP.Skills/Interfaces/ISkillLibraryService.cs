namespace ReasonMCP.Skills.Interfaces
{
    public interface ISkillLibraryService
    {
        Task<string> GetAvailableCategoriesAsync(
            CancellationToken cancellationToken = default
        );

        Task<string> GetSkillsInCategoryAsync(
            string categoryName,
            CancellationToken cancellationToken = default
        );

        Task<string> GetSpecificSkillContentAsync(
            string categoryName,
            string skillName,
            CancellationToken cancellationToken = default
        );
    }
}