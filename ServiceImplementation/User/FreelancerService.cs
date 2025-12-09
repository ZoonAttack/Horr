using ServiceContracts.DTOs.User.Freelancer;
using Entities.Users;
using ServiceImplementation.Authentication.Helpers;
using Services;

namespace ServiceImplementation.Authentication.User
{
    public class FreelancerService : IFreelancerService
    {
        public FreelancerReadDTO freelancer_to_read(Freelancer freelancer)
        {
            return new FreelancerReadDTO
            {
                Id = freelancer.UserId.ToString(),
                Bio = freelancer.Bio,
                HourlyRate = freelancer.HourlyRate,
                Availability = freelancer.Availability,
                YearsOfExperience = freelancer.YearsOfExperience,
                PortfolioUrl = freelancer.PortfolioUrl,
                CreatedAt = freelancer.CreatedAt,
                UpdatedAt = freelancer.UpdatedAt
            };
        }

        public Task<FreelancerReadDTO> CreateFreelancerAsync(FreelancerCreateDTO freelancerCreationDTO)
        {
            if (freelancerCreationDTO == null)
            {
                throw new ArgumentNullException(nameof(freelancerCreationDTO));
            }

            ValidationHelper.ModelValidation(freelancerCreationDTO);

            Freelancer freelancer = freelancerCreationDTO.FreelancerCreate_To_User().Freelancer;

            freelancer.UserId = Guid.NewGuid().ToString();

            return Task.FromResult(freelancer_to_read(freelancer));
        }

        public Task<bool> DeleteFreelancerAsync(Guid freelancerId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<FreelancerReadDTO>> GetAllFreelancersAsync(List<Guid>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<FreelancerReadDTO?> GetFreelancerProfileByIdAsync(Guid freelancerId)
        {
            throw new NotImplementedException();
        }

        public Task<FreelancerReadDTO?> GetFreelancerPublicProfileByIdAsync(Guid freelancerId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<FreelancerReadDTO>> SearchFreelancersAsync(string searchQuery, List<Guid>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateFreelancerAsync(FreelancerUpdateDTO freelancerUpdateDTO)
        {
            throw new NotImplementedException();
        }
    }
}
