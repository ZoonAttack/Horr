using Entities.Skill;
using ServiceContracts.DTOs.Skill.Skill;

namespace Services.Skill
{
    public interface ISkillService
    {
        /// <summary>
        /// Gets all the skills available in the system
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SkillReadDTO>> GetAllSkillsAsync();
    }
}
