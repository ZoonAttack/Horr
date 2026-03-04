using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Skill.Skill
{
    /// <summary>
    /// DTO for updating existing Skill details.
    /// </summary>
    public class SkillUpdateDTO
    {
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }
    }
}
