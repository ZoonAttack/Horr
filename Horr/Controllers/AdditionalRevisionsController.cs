using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Implementations.Contracts;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/revisions/additional")]
    public class AdditionalRevisionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdditionalRevisionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public class RequestAdditionalRevisionBody
        {
            public Guid DeliveryId { get; set; }
            public int RequestedCount { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        [HttpPost("request")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(AdditionalRevisionRequestDto), 200)]
        [ProducesResponseType(typeof(Result<AdditionalRevisionRequestDto>), 400)]
        public async Task<IActionResult> RequestAdditionalRevision([FromBody] RequestAdditionalRevisionBody body)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new RequestAdditionalRevisionCommand(body.DeliveryId, clientId, body.RequestedCount, body.Reason);
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(typeof(IEnumerable<AdditionalRevisionRequestDto>), 200)]
        [ProducesResponseType(typeof(Result<IEnumerable<AdditionalRevisionRequestDto>>), 400)]
        public async Task<IActionResult> GetPendingAdditionalRevisions()
        {
            var freelancerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _mediator.Send(new GetPendingAdditionalRevisionsQuery(freelancerId));

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        public class RespondAdditionalRevisionBody
        {
            public bool Accept { get; set; }
        }

        [HttpPost("{requestId}/respond")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(Result<bool>), 400)]
        public async Task<IActionResult> RespondToRequest(Guid requestId, [FromBody] RespondAdditionalRevisionBody body)
        {
            var freelancerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new RespondToAdditionalRevisionCommand(requestId, freelancerId, body.Accept);
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(result.Data);
        }
    }
}