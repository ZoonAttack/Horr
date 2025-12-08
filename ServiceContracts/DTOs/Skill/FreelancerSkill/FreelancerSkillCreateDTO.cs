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
        public string FreelancerId { get; set; }

        [Required]
        public string SkillId { get; set; }

        [Required]
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }
}
