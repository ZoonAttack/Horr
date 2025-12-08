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

        /// <summary>
        /// Converts SkillCreateDTO to Skill entity
        /// </summary>
        public static Entities.Skill.Skill SkillCreate_To_Skill(this SkillCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Skill.Skill
            {
                Name = createDto.Name,
                Category = createDto.Category
            };
        }

        /// <summary>
        /// Applies SkillUpdateDTO to an existing Skill entity
        /// </summary>
        public static void SkillUpdate_To_Skill(this Entities.Skill.Skill skill, SkillUpdateDTO updateDto)
        {
            if (skill == null || updateDto == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(updateDto.Name))
                skill.Name = updateDto.Name;

            if (!string.IsNullOrEmpty(updateDto.Category))
                skill.Category = updateDto.Category;
        }
    }
}
