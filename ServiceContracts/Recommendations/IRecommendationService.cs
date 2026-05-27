using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceContracts.DTOs.Recommendations;

namespace ServiceContracts.Recommendations
{
    public interface IRecommendationService
    {
        Task<List<RecommendedJobDTO>> GetRecommendedJobsForFreelancerAsync(string userId);
        Task<List<RecommendedFreelancerDTO>> GetRecommendedFreelancersForClientAsync(string userId);
        Task TrackInteractionAsync(string userId, TrackInteractionDTO interactionDto);
    }
}
