using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceContracts.DTOs.FreelancerProfile;
using ServiceImplementation.Repositories.FreelancerProfile;
using ServiceImplementation.Mappings.FreelancerProfile;
using Services.Freelancer.FreelancerProfile;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace ServiceImplementation.Implementations.FreelancerImplementation.FreelancerProfile
{

    public class ExperienceService : IExperienceService
    {
        private readonly IExperienceRepository _experienceRepository;
        private readonly AppDbContext _context;

        public ExperienceService(IExperienceRepository experienceRepository, AppDbContext context)
        {
            _experienceRepository = experienceRepository;
            _context = context;
        }

        public async Task<Result<IEnumerable<ExperienceResponseDto>>> GetUserExperienceAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<IEnumerable<ExperienceResponseDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var experiences = await _experienceRepository.GetByUserIdAsync(userId);
            return new Result<IEnumerable<ExperienceResponseDto>>
            {
                Succeeded = true,
                Data = experiences.ToDtoList()
            };
        }

        public async Task<Result<bool>> SoftDeleteExperienceAsync(string id)
        {
            var experience = await _experienceRepository.GetByIdAsync(id);
            if (experience == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = "EXPERIENCE_NOT_FOUND",
                    Message = "Experience not found."
                };
            }

            // Note: We might want to check the user of this experience here too if we have the userId
            // For now, sticking to basic IsDeleted if possible, but we don't have user context easily here
            // unless we change the signature to include userId.

            experience.IsDeleted = true;
            await _experienceRepository.UpdateAsync(experience);

            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
