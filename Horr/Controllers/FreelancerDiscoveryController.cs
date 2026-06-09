using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using MediatR;
using ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/client/freelancers")]
    [Authorize(Policy = "ClientOnly")] // Restrict to Clients as requested
    public class FreelancerDiscoveryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FreelancerDiscoveryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Searches for freelancers based on various filters.
        /// </summary>
        /// <param name="searchQuery">The text query to search.</param>
        /// <param name="skillIds">Optional list of skill IDs to filter by.</param>
        /// <param name="minHourlyRate">Optional minimum hourly rate.</param>
        /// <param name="maxHourlyRate">Optional maximum hourly rate.</param>
        /// <param name="minYearsExperience">Optional minimum years of experience.</param>
        /// <param name="minTrustScore">Optional minimum trust score.</param>
        /// <param name="isVerified">Optional verification status filter.</param>
        /// <param name="sortBy">Sorting column (default "TrustScore").</param>
        /// <param name="sortDescending">Whether to sort in descending order (default true).</param>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 10).</param>
        /// <returns>A paged list of freelancer search results.</returns>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<FreelancerSearchResultDTO>>> SearchFreelancers(
            [FromQuery] string? searchQuery,
            [FromQuery] List<string>? skillIds,
            [FromQuery] decimal? minHourlyRate,
            [FromQuery] decimal? maxHourlyRate,
            [FromQuery] int? minYearsExperience,
            [FromQuery] decimal? minTrustScore,
            [FromQuery] bool? isVerified,
            [FromQuery] string? sortBy = "TrustScore",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            searchQuery ??= string.Empty;

            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);

            var query = new SearchFreelancersQuery(
                searchQuery, skillIds, minHourlyRate, maxHourlyRate, 
                minYearsExperience, minTrustScore, isVerified, 
                sortBy, sortDescending, page, pageSize, clientId);

            var result = await _mediator.Send(query);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Saves a freelancer to the client's saved list.
        /// </summary>
        /// <param name="freelancerId">The freelancer ID to save.</param>
        /// <returns>A success message.</returns>
        [HttpPost("{freelancerId}/save")]
        public async Task<IActionResult> SaveFreelancer(string freelancerId)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _mediator.Send(new SaveFreelancerCommand(clientId, freelancerId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(new { message = "Freelancer saved successfully." });
        }

        /// <summary>
        /// Removes a freelancer from the client's saved list.
        /// </summary>
        /// <param name="freelancerId">The freelancer ID to unsave.</param>
        /// <returns>A success message.</returns>
        [HttpDelete("{freelancerId}/unsave")]
        public async Task<IActionResult> UnsaveFreelancer(string freelancerId)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _mediator.Send(new UnsaveFreelancerCommand(clientId, freelancerId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(new { message = "Freelancer removed from saved list." });
        }

        /// <summary>
        /// Retrieves the list of saved freelancers for the logged-in client.
        /// </summary>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 10).</param>
        /// <returns>A paged list of saved freelancers.</returns>
        [HttpGet("saved")]
        public async Task<ActionResult<PagedResult<FreelancerSearchResultDTO>>> GetSavedFreelancers(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _mediator.Send(new GetSavedFreelancersQuery(clientId, page, pageSize));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }
    }
}
