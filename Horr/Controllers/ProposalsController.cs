using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Proposal;
using ServiceImplementation.Implementations.Proposals;
using System.Security.Claims;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProposalsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProposalsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submits a new proposal for a job post.
        /// </summary>
        /// <param name="dto">The details of the proposal to submit.</param>
        /// <returns>The created proposal details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProposalReadDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create(ProposalCreateDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new CreateProposalCommand(dto, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetMyProposals), new { id = result.Data.Id }, result.Data);
        }

        /// <summary>
        /// Retrieves the list of proposals submitted by the logged-in freelancer.
        /// </summary>
        /// <returns>The freelancer's submitted proposals.</returns>
        [HttpGet("my-proposals")]
        [ProducesResponseType(typeof(MyProposalsResponseDto), 200)]
        public async Task<IActionResult> GetMyProposals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyProposalsQuery(userId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Withdraws a submitted proposal.
        /// </summary>
        /// <param name="id">The proposal ID to withdraw.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}/withdraw")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new WithdrawProposalCommand(id, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return NoContent();
        }

        /// <summary>
        /// Rejects a submitted proposal.
        /// </summary>
        /// <param name="id">The proposal ID to reject.</param>
        /// <returns>No content on success.</returns>
        [HttpPost("{id}/reject")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new RejectProposalCommand(id, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return NoContent();
        }
    }
}
