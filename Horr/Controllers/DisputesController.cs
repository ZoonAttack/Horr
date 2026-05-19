using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Implementations.Contracts;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/disputes")]
    public class DisputesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DisputesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public class ResolveDisputeRequest
        {
            public DisputeDecision Decision { get; set; }
            public string AdminDecision { get; set; } = string.Empty;
        }

        [HttpPost("{disputeId}/resolve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DisputeDto), 200)]
        public async Task<IActionResult> Resolve(Guid disputeId, [FromBody] ResolveDisputeRequest req)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new ResolveDisputeCommand(disputeId, req.Decision, req.AdminDecision, adminId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
