using ServiceContracts.DTOs.Skill.FreelancerSkill;
using ServiceContracts.DTOs.Skill.Skill;

namespace Services.Skill
{
    public interface IFreelancerSkillService
    {
        /// <summary>
        /// Initial assignment of skills
        /// </summary>
        Task AddSkillsAsync(string freelancerId, IEnumerable<FreelancerSkillCreateDTO> skills);

        /// <summary>
        /// Syncs the freelancer's skills with the provided updated skills list
        /// </summary>
        Task<IEnumerable<SkillReadDTO>> UpdateSkillsAsync(string freelancerId, IEnumerable<FreelancerSkillCreateDTO> updatedSkills);
    }
}