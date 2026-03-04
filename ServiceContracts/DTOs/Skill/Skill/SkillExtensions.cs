namespace ServiceContracts.DTOs.Skill.Skill
{
    public static class SkillExtensions
    {
        /// <summary>
        /// Converts Skill entity to SkillReadDTO
        /// </summary>
        public static SkillReadDTO Skill_To_SkillRead(this Entities.Skill.Skill skill)
        {
            if (skill == null)
            {
                return null;
            }

            return new SkillReadDTO
            {
                Id = skill.Id.ToString(),
                Name = skill.Name,
                Category = skill.Category,
                CreatedAt = skill.CreatedAt,
                UpdatedAt = skill.UpdatedAt
            };
        }
    }
}
