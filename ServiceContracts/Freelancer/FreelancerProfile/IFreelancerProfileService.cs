using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceContracts.DTOs.FreelancerProfile;

namespace Services.Freelancer.FreelancerProfile
{
    public interface IPortfolioService
    {
        Task<IEnumerable<PortfolioResponseDto>> GetUserPortfolioAsync(string userId);
        Task<PortfolioResponseDto> CreatePortfolioItemAsync(string userId, PortfolioCreateDto dto);
    }

    public interface IExperienceService
    {
        Task<IEnumerable<ExperienceResponseDto>> GetUserExperienceAsync(string userId);
        Task<bool> SoftDeleteExperienceAsync(string id);
    }
}
