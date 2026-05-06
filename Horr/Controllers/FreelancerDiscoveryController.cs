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

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<FreelancerReadDTO>>> SearchFreelancers(
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

        [HttpPost("{freelancerId}/save")]
        public async Task<IActionResult> SaveFreelancer(string freelancerId)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _mediator.Send(new SaveFreelancerCommand(clientId, freelancerId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(new { message = "Freelancer saved successfully." });
        }

        [HttpDelete("{freelancerId}/unsave")]
        public async Task<IActionResult> UnsaveFreelancer(string freelancerId)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _mediator.Send(new UnsaveFreelancerCommand(clientId, freelancerId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(new { message = "Freelancer removed from saved list." });
        }

        [HttpGet("saved")]
        public async Task<ActionResult<PagedResult<FreelancerReadDTO>>> GetSavedFreelancers(
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
