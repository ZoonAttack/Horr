using Entities.Enums;

namespace ServiceContracts.DTOs.Skill.FreelancerSkill
{
    /// <summary>
    /// DTO for reading or displaying freelancer skill association.
    /// </summary>
    public class FreelancerSkillReadDTO
    {
        public long FreelancerId { get; set; }

        public long SkillId { get; set; }

        public string SkillName { get; set; }

        public string SkillCategory { get; set; }

        public ProficiencyLevel ProficiencyLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
