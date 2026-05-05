using Entities;
using Entities.Skill;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Skill;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SkillsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAllSkills()
        {
            var skills = await _context.Skills
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category
                })
                .ToListAsync();

            return Ok(skills);
        }

        [HttpGet("my-skills")]
        public async Task<ActionResult<IEnumerable<FreelancerSkillDto>>> GetMySkills()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return NotFound("Freelancer profile not found.");

            var mySkills = await _context.FreelancerSkills
                .Where(fs => fs.FreelancerId == freelancer.UserId)
                .Select(fs => new FreelancerSkillDto
                {
                    SkillId = fs.SkillId,
                    SkillName = fs.Skill.Name,
                    SkillCategory = fs.Skill.Category,
                    ProficiencyLevel = (int)fs.ProficiencyLevel
                })
                .ToListAsync();

            return Ok(mySkills);
        }

        [HttpPost("my-skills")]
        public async Task<ActionResult<FreelancerSkillDto>> AddMySkill([FromBody] AddFreelancerSkillDto dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return NotFound("Freelancer profile not found.");

            var skill = await _context.Skills.FindAsync(dto.SkillId);
            if (skill == null) return NotFound("Skill not found.");

            var existing = await _context.FreelancerSkills
                .FirstOrDefaultAsync(fs => fs.FreelancerId == freelancer.UserId && fs.SkillId == dto.SkillId);
            if (existing != null) return Conflict("Skill already added to profile.");

            var freelancerSkill = new FreelancerSkill
            {
                FreelancerId = freelancer.UserId,
                SkillId = dto.SkillId,
                ProficiencyLevel = (Entities.Enums.ProficiencyLevel)dto.ProficiencyLevel
            };

            _context.FreelancerSkills.Add(freelancerSkill);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMySkills), new FreelancerSkillDto
            {
                SkillId = freelancerSkill.SkillId,
                SkillName = skill.Name,
                SkillCategory = skill.Category,
                ProficiencyLevel = (int)freelancerSkill.ProficiencyLevel
            });
        }

        [HttpDelete("my-skills/{skillId}")]
        public async Task<IActionResult> DeleteMySkill(string skillId)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return NotFound("Freelancer profile not found.");

            var freelancerSkill = await _context.FreelancerSkills
                .FirstOrDefaultAsync(fs => fs.FreelancerId == freelancer.UserId && fs.SkillId == skillId);
            if (freelancerSkill == null) return NotFound("Skill not found in freelancer profile.");

            _context.FreelancerSkills.Remove(freelancerSkill);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
