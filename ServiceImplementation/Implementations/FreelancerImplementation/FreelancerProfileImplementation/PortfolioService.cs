using ServiceContracts.DTOs.FreelancerProfile;
using ServiceImplementation.Repositories.FreelancerProfile;
using ServiceImplementation.Mappings.FreelancerProfile;
using Services.Freelancer.FreelancerProfile;

namespace ServiceImplementation.Implementations.Freelancer.FreelancerProfile
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
}
