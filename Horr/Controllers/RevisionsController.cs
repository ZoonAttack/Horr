using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Implementations.Contracts;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RevisionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RevisionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("open")]
        [Authorize(Roles = "Specialist")]
        [ProducesResponseType(typeof(IEnumerable<RevisionRequestDto>), 200)]
        public async Task<IActionResult> GetOpenRevisions()
        {
            var result = await _mediator.Send(new GetRevisionRequestsQuery());
            return Ok(result);
        }

        [HttpGet("specialist-queue")]
        [Authorize(Roles = "Specialist")]
        [ProducesResponseType(typeof(List<ContractSpecialistReviewReadDto>), 200)]
        public async Task<IActionResult> GetSpecialistQueue()
        {
            var specialistId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _mediator.Send(new GetMyPendingSpecialistReviewsQuery(specialistId));

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpGet("freelancer")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(typeof(List<RevisionRequestDto>), 200)]
        public async Task<IActionResult> GetFreelancerRevisions([FromQuery] int? contractId)
        {
            var freelancerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _mediator.Send(new GetFreelancerRevisionRequestsQuery(freelancerId, contractId));

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(result.Data);
        }
    }
}
