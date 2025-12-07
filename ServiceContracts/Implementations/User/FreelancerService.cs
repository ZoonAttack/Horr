using ServiceContracts.DTOs.User.Freelancer;
using ServiceContracts.User;
using Services.Helpers;
using Entities.User;

namespace Services.Implementations.User
{
    public class FreelancerService : IFreelancerService
    {
        public Task<Guid> CreateFreelancerAsync(FreelancerCreateDTO freelancerCreationDTO)
        {
            if (freelancerCreationDTO == null)
            {
                throw new ArgumentNullException(nameof(freelancerCreationDTO));
            }

            ValidationHelper.ModelValidation(freelancerCreationDTO);

            Freelancer freelancer = freelancerCreationDTO.FreelancerCreate_To_User().Freelancer;

            freelancer.UserId = Guid.NewGuid();

            return Task.FromResult(freelancer.UserId);
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
