using SkillSync.Api.DTOs.Skills;

namespace SkillSync.Api.Services.Skills;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> GetAllSkillsAsync(string userId);
    Task<SkillDto?> GetSkillByIdAsync(int id, string userId);
    Task<SkillDto> CreateSkillAsync(CreateSkillDto dto, string userId);
    Task<SkillDto?> UpdateSkillAsync(int id, UpdateSkillDto dto, string userId);
    Task<bool> DeleteSkillAsync(int id, string userId);
    Task<IEnumerable<SkillCategoryDto>> GetCategoriesAsync();
    Task<IEnumerable<SkillDto>> GetSkillsByCategoryAsync(int categoryId, string userId);
}