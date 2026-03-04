using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Skill.Skill
{
    /// <summary>
    /// DTO for creating a new Skill.
    /// </summary>
    public class SkillCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }
    }
}
