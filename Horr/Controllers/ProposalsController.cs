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

        [HttpPost]
        [ProducesResponseType(typeof(ProposalReadDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create(ProposalCreateDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new CreateProposalCommand(dto, userId));
            return CreatedAtAction(nameof(GetMyProposals), new { id = result.Id }, result);
        }

        [HttpGet("my-proposals")]
        [ProducesResponseType(typeof(MyProposalsResponseDto), 200)]
        public async Task<IActionResult> GetMyProposals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyProposalsQuery(userId));
            return Ok(result);
        }

        [HttpDelete("{id}/withdraw")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _mediator.Send(new WithdrawProposalCommand(id, userId));
            return NoContent();
        }
    }
}
