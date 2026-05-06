using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceContracts.DTOs.FreelancerProfile;
using ServiceContracts.DTOs.Responses;

namespace Services.Freelancer.FreelancerProfile
{
    public interface IPortfolioService
    {
        Task<Result<IEnumerable<PortfolioResponseDto>>> GetUserPortfolioAsync(string userId);
        Task<Result<PortfolioResponseDto>> CreatePortfolioItemAsync(string userId, PortfolioCreateDto dto);
    }

    public interface IExperienceService
    {
        Task<Result<IEnumerable<ExperienceResponseDto>>> GetUserExperienceAsync(string userId);
        Task<Result<bool>> SoftDeleteExperienceAsync(string id);
    }
}
