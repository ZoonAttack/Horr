using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Skill
{
    public class SkillDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public string? CategoryId { get; set; }
    }

    public class FreelancerSkillDto
    {
        public string SkillId { get; set; }
        public string SkillName { get; set; }
        public string? SkillCategory { get; set; }
        public string? SkillCategoryId { get; set; }
        public int ProficiencyLevel { get; set; }
    }

    public class AddFreelancerSkillDto
    {
        [Required]
        public string SkillId { get; set; }
        [Range(0, 3)]
        public int ProficiencyLevel { get; set; }
    }
}
