using System.ComponentModel.DataAnnotations;
using Entities.Enums;

namespace ServiceContracts.DTOs.Skill.FreelancerSkill
{
    /// <summary>
    /// DTO for creating a new FreelancerSkill association.
    /// </summary>
    public class FreelancerSkillCreateDTO
    {
        [Required]
        public long FreelancerId { get; set; }

        [Required]
        public long SkillId { get; set; }

        [Required]
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }
}
