using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceImplementation.Implementations.Contracts;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MilestonesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MilestonesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{milestoneId}/fund")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Fund(Guid milestoneId)
        {
            var clientIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(clientIdString, out var clientId))
            {
                return BadRequest("Invalid Client ID format.");
            }

            var command = new FundMilestoneCommand(milestoneId, clientId);
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }
            return Ok();
        }
    }
}
