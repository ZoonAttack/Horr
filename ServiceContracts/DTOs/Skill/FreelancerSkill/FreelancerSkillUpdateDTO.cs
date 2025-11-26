using Entities.Enums;

namespace ServiceContracts.DTOs.Skill.FreelancerSkill
{
    /// <summary>
    /// DTO for updating FreelancerSkill proficiency level.
    /// </summary>
    public class FreelancerSkillUpdateDTO
    {
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }
}
