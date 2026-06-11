using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.Client;
using Services.Client;
using ServiceContracts.DTOs.Proposal;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/client")]
    [Authorize(Policy = "ClientOnly")]
    public class ClientController : ControllerBase
    {
        private readonly IClientProfileService _profileService;
        private readonly IJobService _jobService;

        public ClientController(IClientProfileService profileService, IJobService jobService)
        {
            _profileService = profileService;
            _jobService = jobService;
        }

        /// <summary>
        /// Retrieves the profile details of the logged-in client.
        /// </summary>
        /// <returns>The client profile details.</returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _profileService.GetClientMeAsync(userId);
            if (result.Succeeded) return Ok(result.Data);
            return BadRequest(new { result.ErrorCode, result.Message });
        }

        /// <summary>
        /// Retrieves onboarding data for the logged-in client.
        /// </summary>
        /// <returns>The onboarding details.</returns>
        [HttpGet("onboarding")]
        public async Task<IActionResult> GetOnboarding()
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _profileService.GetClientOnboardingAsync(userId);
            if (result.Succeeded) return Ok(result.Data);
            return BadRequest(new { result.ErrorCode, result.Message });
        }

        /// <summary>
        /// Retrieves all jobs created by the logged-in client.
        /// </summary>
        /// <returns>A list of jobs created by the client.</returns>
        [HttpGet("jobs")]
        public async Task<IActionResult> GetClientJobs()
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _jobService.GetClientJobsAsync(userId);
            if (result.Succeeded) return Ok(result.Data);
            return BadRequest(new { result.ErrorCode, result.Message });
        }

        /// <summary>
        /// Retrieves all proposals submitted to jobs created by the logged-in client.
        /// </summary>
        /// <returns>A list of proposals directed to the client.</returns>
        [HttpGet("proposals")]
        [ProducesResponseType(typeof(IEnumerable<ClientProposalSummaryDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetClientProposals()
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _jobService.GetClientProposalsAsync(userId);
            if (result.Succeeded) return Ok(result.Data);
            return BadRequest(new { result.ErrorCode, result.Message });
        }
    }
}
