using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceContracts.DTOs.FreelancerProfile;
using ServiceContracts.FreelancerProfile;
using ServiceImplementation.Repositories.FreelancerProfile;
using ServiceImplementation.Mappings.FreelancerProfile;

namespace ServiceImplementation.Services.FreelancerProfile
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepository;

        public PortfolioService(IPortfolioRepository portfolioRepository)
        {
            _portfolioRepository = portfolioRepository;
        }

        public async Task<IEnumerable<PortfolioResponseDto>> GetUserPortfolioAsync(Guid userId)
        {
            var items = await _portfolioRepository.GetByUserIdAsync(userId);
            return items.ToDtoList();
        }

        public async Task<PortfolioResponseDto> CreatePortfolioItemAsync(Guid userId, PortfolioCreateDto dto)
        {
            var entity = dto.ToEntity(userId);
            var result = await _portfolioRepository.AddAsync(entity);
            return result.ToDto();
        }
    }

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
