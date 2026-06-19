using Entities.Enums;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.Client;
using ServiceContracts.DTOs.JobInvitation;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobInvitationsController : ControllerBase
    {
        private readonly IJobInvitationService _invitationService;

        public JobInvitationsController(IJobInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        /// <summary>
        /// Sends a job invitation to a freelancer. Accessible by Clients only.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(Result<JobInvitationReadDto>), 201)]
        [ProducesResponseType(typeof(Result<JobInvitationReadDto>), 400)]
        public async Task<IActionResult> SendInvitation([FromBody] JobInvitationCreateDto dto)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _invitationService.SendInvitationAsync(clientId, dto);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetInvitation), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Withdraws a sent job invitation. Accessible by Clients only.
        /// </summary>
        [HttpPost("{id}/withdraw")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(Result<bool>), 200)]
        [ProducesResponseType(typeof(Result<bool>), 400)]
        public async Task<IActionResult> WithdrawInvitation(string id)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _invitationService.WithdrawInvitationAsync(clientId, id);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Declines a job invitation. Accessible by Freelancers only.
        /// </summary>
        [HttpPost("{id}/decline")]
        [Authorize(Policy = "FreelancerOnly")]
        [ProducesResponseType(typeof(Result<bool>), 200)]
        [ProducesResponseType(typeof(Result<bool>), 400)]
        public async Task<IActionResult> DeclineInvitation(string id)
        {
            string freelancerId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(freelancerId)) return Unauthorized();

            var result = await _invitationService.DeclineInvitationAsync(freelancerId, id);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the details of a specific job invitation.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<JobInvitationReadDto>), 200)]
        [ProducesResponseType(typeof(Result<JobInvitationReadDto>), 400)]
        public async Task<IActionResult> GetInvitation(string id)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _invitationService.GetInvitationDetailsAsync(userId, id);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves invitations sent by the logged-in client.
        /// </summary>
        [HttpGet("client")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(Result<List<JobInvitationReadDto>>), 200)]
        public async Task<IActionResult> GetClientInvitations([FromQuery] string? jobPostId = null)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _invitationService.GetClientInvitationsAsync(clientId, jobPostId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves invitations received by the logged-in freelancer.
        /// </summary>
        [HttpGet("freelancer")]
        [Authorize(Policy = "FreelancerOnly")]
        [ProducesResponseType(typeof(Result<List<JobInvitationReadDto>>), 200)]
        public async Task<IActionResult> GetFreelancerInvitations([FromQuery] InvitationStatus? status = null)
        {
            string freelancerId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(freelancerId)) return Unauthorized();

            var result = await _invitationService.GetFreelancerInvitationsAsync(freelancerId, status);
            return Ok(result);
        }
    }
}
