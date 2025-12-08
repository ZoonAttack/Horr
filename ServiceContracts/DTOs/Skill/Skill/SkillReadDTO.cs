namespace ServiceContracts.DTOs.Skill.Skill
{
    /// <summary>
    /// DTO for reading or displaying skill information.
    /// </summary>
    public class SkillReadDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
