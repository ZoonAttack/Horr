using Entities;
using Entities.Skill;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Skill.FreelancerSkill;
using ServiceContracts.DTOs.Skill.Skill;
using Services.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceImplementation.Skills
{
    public class FreelancerSkillService : IFreelancerSkillService
    {
        private readonly AppDbContext _db;
        public FreelancerSkillService(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddSkillsAsync(string freelancerId, IEnumerable<FreelancerSkillCreateDTO> skills)
        {
            var newEntries = skills.Select(s => new FreelancerSkill
            {
                FreelancerId = freelancerId,
                SkillId = s.SkillId
            });

            await _db.FreelancerSkills.AddRangeAsync(newEntries);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<SkillReadDTO>> UpdateSkillsAsync(string freelancerId, IEnumerable<FreelancerSkillCreateDTO> updatedSkills)
        {
            // 1. Get current skills from DB
            var existingSkills = await _db.FreelancerSkills
                .Where(fs => fs.FreelancerId == freelancerId)
                .ToListAsync();

            // 2. Identify IDs sent from the UI
            var newSkillIds = updatedSkills.Select(s => s.SkillId).ToList();

            // 3. Remove skills that are in DB but NOT in the new list (Unchecked in UI)
            var skillsToRemove = existingSkills.Where(es => !newSkillIds.Contains(es.SkillId));
            _db.FreelancerSkills.RemoveRange(skillsToRemove);

            // 4. Add skills that are in the new list but NOT in the DB (Newly checked in UI)
            var existingSkillIds = existingSkills.Select(es => es.SkillId).ToList();
            var idsToAdd = newSkillIds.Where(id => !existingSkillIds.Contains(id));

            var skillsToAdd = idsToAdd.Select(id => new FreelancerSkill
            {
                FreelancerId = freelancerId,
                SkillId = id
            });

            await _db.FreelancerSkills.AddRangeAsync(skillsToAdd);

            // 5. Save all changes in one transaction
            await _db.SaveChangesAsync();

            // 6. Return the updated list with full Skill details (Names, Categories)
            return await _db.FreelancerSkills
                .AsNoTracking()
                .Where(fs => fs.FreelancerId == freelancerId)
                .Select(fs => fs.Skill.Skill_To_SkillRead())
                .ToListAsync();
        }
    }
}
