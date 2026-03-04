using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceContracts.DTOs.FreelancerProfile;

namespace Services.Freelancer.FreelancerProfile
{
    public interface IPortfolioService
    {
        Task<IEnumerable<PortfolioResponseDto>> GetUserPortfolioAsync(Guid userId);
        Task<PortfolioResponseDto> CreatePortfolioItemAsync(Guid userId, PortfolioCreateDto dto);
    }

    public interface IExperienceService
    {
        Task<IEnumerable<ExperienceResponseDto>> GetUserExperienceAsync(Guid userId);
        Task<bool> SoftDeleteExperienceAsync(Guid id);
    }
}
