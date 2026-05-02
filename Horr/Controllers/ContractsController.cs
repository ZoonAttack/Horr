using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Review;
using ServiceImplementation.Implementations.Contracts;
using ServiceImplementation.Implementations.Reviews;
using System.Security.Claims;
using Entities.Enums;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContractsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("my-contracts")]
        [ProducesResponseType(typeof(IEnumerable<ContractReadDTO>), 200)]
        public async Task<IActionResult> GetMyContracts(
            [FromQuery] ContractStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Extract role from JWT claims (role claim or custom "UserRole" claim)
            var userRole = User.FindFirstValue(ClaimTypes.Role)
                        ?? User.FindFirstValue("UserRole")
                        ?? "Freelancer";

            var result = await _mediator.Send(new GetMyContractsQuery(userId, userRole, status, page, pageSize));
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContractReadDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetContractById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetContractByIdQuery(id, userId));
            return Ok(result);
        }

        [HttpPost("{id}/accept-offer")]
        [ProducesResponseType(typeof(bool), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> AcceptOffer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new AcceptOfferCommand(id, userId));
            return StatusCode(201, result);
        }

        [HttpPost("{id}/decline-offer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> DeclineOffer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _mediator.Send(new DeclineOfferCommand(id, userId));
            return NoContent();
        }

        [HttpPost("{id}/deliver-work")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(WorkDeliveryDto), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> DeliverWork(int id, [FromForm] string note, [FromForm] List<IFormFile> files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new DeliverWorkCommand(id, note, userId, files));
            return StatusCode(201, result);
        }

        [HttpPost("{id}/complete")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CompleteContract(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _mediator.Send(new MarkContractAsCompletedCommand(id, userId));
            return NoContent();
        }

        [HttpPost("{id}/reject")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> RejectContract(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _mediator.Send(new RejectContractCommand(id, userId));
            return NoContent();
        }

        [HttpPost("{id}/reviews")]
        [ProducesResponseType(typeof(ContractReviewReadDTO), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> SubmitReview(int id, ContractReviewCreateDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new SubmitReviewCommand(id, dto, userId));
            return StatusCode(201, result);
        }
    }
}
