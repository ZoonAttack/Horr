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
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly AppDbContext _context;

        public PortfolioService(IPortfolioRepository portfolioRepository, AppDbContext context)
        {
            _portfolioRepository = portfolioRepository;
            _context = context;
        }

        public async Task<Result<IEnumerable<PortfolioResponseDto>>> GetUserPortfolioAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<IEnumerable<PortfolioResponseDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var items = await _portfolioRepository.GetByUserIdAsync(userId);
            return new Result<IEnumerable<PortfolioResponseDto>>
            {
                Succeeded = true,
                Data = items.ToDtoList()
            };
        }

        public async Task<Result<PortfolioResponseDto>> CreatePortfolioItemAsync(string userId, PortfolioCreateDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<PortfolioResponseDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var entity = dto.ToEntity(userId);
            var result = await _portfolioRepository.AddAsync(entity);
            return new Result<PortfolioResponseDto>
            {
                Succeeded = true,
                Data = result.ToDto()
            };
        }
    }
}
