namespace ServiceContracts.DTOs.Skill.FreelancerSkill
{
    public static class FreelancerSkillExtensions
    {
        /// <summary>
        /// Converts FreelancerSkill entity to FreelancerSkillReadDTO
        /// </summary>
        public static FreelancerSkillReadDTO FreelancerSkill_To_FreelancerSkillRead(this Entities.Skill.FreelancerSkill freelancerSkill)
        {
            if (freelancerSkill == null)
            {
                return null;
            }

            return new FreelancerSkillReadDTO
            {
                FreelancerId = freelancerSkill.FreelancerId,
                SkillId = freelancerSkill.SkillId,
                SkillName = freelancerSkill.Skill?.Name,
                SkillCategory = freelancerSkill.Skill?.Category,
                ProficiencyLevel = freelancerSkill.ProficiencyLevel,
                CreatedAt = freelancerSkill.CreatedAt,
                UpdatedAt = freelancerSkill.UpdatedAt
            };
        }

        /// <summary>
        /// Converts FreelancerSkillCreateDTO to FreelancerSkill entity
        /// </summary>
        public static Entities.Skill.FreelancerSkill FreelancerSkillCreate_To_FreelancerSkill(this FreelancerSkillCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Skill.FreelancerSkill
            {
                FreelancerId = createDto.FreelancerId,
                SkillId = createDto.SkillId,
                ProficiencyLevel = createDto.ProficiencyLevel
            };
        }

        /// <summary>
        /// Applies FreelancerSkillUpdateDTO to an existing FreelancerSkill entity
        /// </summary>
        public static void FreelancerSkillUpdate_To_FreelancerSkill(this Entities.Skill.FreelancerSkill freelancerSkill, FreelancerSkillUpdateDTO updateDto)
        {
            if (freelancerSkill == null || updateDto == null)
            {
                return;
            }

            freelancerSkill.ProficiencyLevel = updateDto.ProficiencyLevel;
        }
    }
}
