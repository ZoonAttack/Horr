using Entities;
using Entities.Skill;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Skill;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations
{
    public class SkillService : ISkillService
    {
        private readonly AppDbContext _context;

        public SkillService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SkillDto>>> GetAllSkillsAsync()
        {
            var skills = await _context.Skills
                .Include(s => s.Category)
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    Category = s.Category != null ? s.Category.Name : null
                })
                .ToListAsync();

            return new Result<List<SkillDto>> { Succeeded = true, Data = skills };
        }

        public async Task<Result<List<SkillDto>>> GetSkillsByCategoryAsync(string categoryId)
        {
            var skills = await _context.Skills
                .Where(s => s.CategoryId == categoryId)
                .Include(s => s.Category)
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    Category = s.Category != null ? s.Category.Name : null
                })
                .ToListAsync();

            return new Result<List<SkillDto>> { Succeeded = true, Data = skills };
        }

        public async Task<Result<List<FreelancerSkillDto>>> GetMySkillsAsync(string userId)
        {
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null)
            {
                return new Result<List<FreelancerSkillDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FreelancerNotFound,
                    Message = "Freelancer profile not found."
                };
            }

            var mySkills = await _context.FreelancerSkills
                .Where(fs => fs.FreelancerId == freelancer.UserId)
                .Include(fs => fs.Skill.Category)
                .Select(fs => new FreelancerSkillDto
                {
                    SkillId = fs.SkillId,
                    SkillName = fs.Skill.Name,
                    SkillCategoryId = fs.Skill.CategoryId,
                    SkillCategory = fs.Skill.Category != null ? fs.Skill.Category.Name : null,
                    ProficiencyLevel = (int)fs.ProficiencyLevel
                })
                .ToListAsync();

            return new Result<List<FreelancerSkillDto>> { Succeeded = true, Data = mySkills };
        }

        public async Task<Result<FreelancerSkillDto>> AddMySkillAsync(string userId, AddFreelancerSkillDto dto)
        {
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null)
            {
                return new Result<FreelancerSkillDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FreelancerNotFound,
                    Message = "Freelancer profile not found."
                };
            }

            var skill = await _context.Skills
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == dto.SkillId);
            if (skill == null)
            {
                return new Result<FreelancerSkillDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.SkillNotFound,
                    Message = "Skill not found."
                };
            }

            var existing = await _context.FreelancerSkills
                .FirstOrDefaultAsync(fs => fs.FreelancerId == freelancer.UserId && fs.SkillId == dto.SkillId);
            if (existing != null)
            {
                return new Result<FreelancerSkillDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.SkillAlreadyAdded,
                    Message = "Skill already added to profile."
                };
            }

            var freelancerSkill = new FreelancerSkill
            {
                FreelancerId = freelancer.UserId,
                SkillId = dto.SkillId,
                ProficiencyLevel = (Entities.Enums.ProficiencyLevel)dto.ProficiencyLevel
            };

            _context.FreelancerSkills.Add(freelancerSkill);
            await _context.SaveChangesAsync();

            return new Result<FreelancerSkillDto>
            {
                Succeeded = true,
                Data = new FreelancerSkillDto
                {
                    SkillId = freelancerSkill.SkillId,
                    SkillName = skill.Name,
                    SkillCategoryId = skill.CategoryId,
                    SkillCategory = skill.Category != null ? skill.Category.Name : null,
                    ProficiencyLevel = (int)freelancerSkill.ProficiencyLevel
                }
            };
        }

        public async Task<Result<bool>> DeleteMySkillAsync(string userId, string skillId)
        {
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FreelancerNotFound,
                    Message = "Freelancer profile not found."
                };
            }

            var freelancerSkill = await _context.FreelancerSkills
                .FirstOrDefaultAsync(fs => fs.FreelancerId == freelancer.UserId && fs.SkillId == skillId);
            if (freelancerSkill == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.SkillNotFound,
                    Message = "Skill not found in freelancer profile."
                };
            }

            _context.FreelancerSkills.Remove(freelancerSkill);
            await _context.SaveChangesAsync();

            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
