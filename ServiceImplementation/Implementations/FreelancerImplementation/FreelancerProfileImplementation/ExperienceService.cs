using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceContracts.DTOs.FreelancerProfile;
using ServiceImplementation.Repositories.FreelancerProfile;
using ServiceImplementation.Mappings.FreelancerProfile;
using Services.Freelancer.FreelancerProfile;

namespace ServiceImplementation.Implementations.Freelancer.FreelancerProfile
{

    public class ExperienceService : IExperienceService
    {
        private readonly IExperienceRepository _experienceRepository;

        public ExperienceService(IExperienceRepository experienceRepository)
        {
            _experienceRepository = experienceRepository;
        }

        public async Task<IEnumerable<ExperienceResponseDto>> GetUserExperienceAsync(Guid userId)
        {
            var experiences = await _experienceRepository.GetByUserIdAsync(userId);
            return experiences.ToDtoList();
        }

        public async Task<bool> SoftDeleteExperienceAsync(Guid id)
        {
            var experience = await _experienceRepository.GetByIdAsync(id);
            if (experience == null) return false;

            experience.IsDeleted = true;
            await _experienceRepository.UpdateAsync(experience);

            return true;
        }
    }
}
