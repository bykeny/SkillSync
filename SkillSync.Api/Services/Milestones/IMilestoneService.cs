using SkillSync.Api.DTOs.Milestones;

namespace SkillSync.Api.Services.Milestones;

public interface IMilestoneService
{
    Task<IEnumerable<MilestoneDto>> GetAllMilestonesAsync(string userId);
    Task<MilestoneDto?> GetMilestoneByIdAsync(int id, string userId);
    Task<MilestoneDto> CreateMilestoneAsync(CreateMilestoneDto dto, string userId);
    Task<MilestoneDto?> UpdateMilestoneAsync(int id, UpdateMilestoneDto dto, string userId);
    Task<bool> DeleteMilestoneAsync(int id, string userId);
    Task<IEnumerable<MilestoneDto>> GetMilestonesBySkillAsync(int skillId, string userId);
    Task<MilestoneDto?> CompleteMilestoneAsync(int id, string userId);
    Task<IEnumerable<MilestoneDto>> GetOverdueMilestonesAsync(string userId);
}