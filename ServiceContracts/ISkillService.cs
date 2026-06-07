using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Skill;

namespace ServiceContracts
{
    public interface ISkillService
    {
        Task<Result<List<SkillDto>>> GetAllSkillsAsync();
        Task<Result<List<SkillDto>>> GetSkillsByCategoryAsync(string categoryId);
        Task<Result<List<FreelancerSkillDto>>> GetMySkillsAsync(string userId);
        Task<Result<FreelancerSkillDto>> AddMySkillAsync(string userId, AddFreelancerSkillDto dto);
        Task<Result<bool>> DeleteMySkillAsync(string userId, string skillId);
    }
}
