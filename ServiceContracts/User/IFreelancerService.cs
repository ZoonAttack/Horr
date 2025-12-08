using ServiceContracts.DTOs.User.Freelancer;

namespace ServiceContracts.User
{
    public interface IFreelancerService
    {
        public Task<FreelancerReadDTO> CreateFreelancerAsync(FreelancerCreateDTO freelancerCreationDTO);

        public Task<bool> UpdateFreelancerAsync(FreelancerUpdateDTO freelancerUpdateDTO);

        /// <summary>
        /// Freelancer viewing their own profile (full data)
        /// </summary>
        public Task<FreelancerReadDTO?> GetFreelancerProfileByIdAsync(Guid freelancerId);

        /// <summary>
        /// Viewing Freelancer public profile (limited data)
        /// </summary>
        public Task<FreelancerReadDTO?> GetFreelancerPublicProfileByIdAsync(Guid freelancerId);

        /// <summary>
        /// Get all freelancers with optional filters 
        /// </summary>
        public Task<PagedResult<FreelancerReadDTO>> GetAllFreelancersAsync(
            List<Guid>? skillIds = null,
            decimal? minHourlyRate = null,
            decimal? maxHourlyRate = null,
            int? minYearsExperience = null,
            decimal? minTrustScore = null,
            bool? isVerified = null,
            string? sortBy = "TrustScore",
            bool sortDescending = true,
            int page = 1,
            int pageSize = 10);

        /// <summary>
        /// Search freelancers by query string
        /// </summary>
        public Task<PagedResult<FreelancerReadDTO>> SearchFreelancersAsync(
            string searchQuery,
            List<Guid>? skillIds = null,
            decimal? minHourlyRate = null,
            decimal? maxHourlyRate = null,
            int? minYearsExperience = null,
            decimal? minTrustScore = null,
            bool? isVerified = null,
            string? sortBy = "TrustScore",
            bool sortDescending = true,
            int page = 1,
            int pageSize = 10);

        public Task<bool> DeleteFreelancerAsync(Guid freelancerId);
    }
}