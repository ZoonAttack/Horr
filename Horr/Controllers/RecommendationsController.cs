using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.Recommendations;
using ServiceContracts.DTOs.Recommendations;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Retrieves recommended job posts for the logged-in freelancer.
        /// </summary>
        /// <returns>A list of recommended jobs.</returns>
        [HttpGet("jobs")]
        [Authorize(Policy = "FreelancerOnly")]
        public async Task<IActionResult> GetRecommendedJobs()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _recommendationService.GetRecommendedJobsForFreelancerAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves recommended freelancers for the logged-in client.
        /// </summary>
        /// <returns>A list of recommended freelancers.</returns>
        [HttpGet("freelancers")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> GetRecommendedFreelancers()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _recommendationService.GetRecommendedFreelancersForClientAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Tracks user interactions (views, clicks, etc.) for recommendation system optimization.
        /// </summary>
        /// <param name="dto">The details of the interaction to track.</param>
        /// <returns>A status indicating success.</returns>
        [HttpPost("track")]
        [Authorize]
        public async Task<IActionResult> Track([FromBody] TrackInteractionDTO dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _recommendationService.TrackInteractionAsync(userId, dto);
            return Ok();
        }
    }
}
