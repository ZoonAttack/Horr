using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Skill.Skill;
using Services.Skill;

namespace ServiceImplementation.Skills
{
    public class SkillsService : ISkillService
    {
        private readonly AppDbContext _db;
        public SkillsService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<SkillReadDTO>> GetAllSkillsAsync()
        {
            return await _db.Skills
                .AsNoTracking()
                .Select(sk => sk.Skill_To_SkillRead())
                .ToListAsync();
        }
    }
}
