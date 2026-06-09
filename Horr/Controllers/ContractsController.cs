using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Review;
using ServiceImplementation.Implementations.Contracts;
using ServiceImplementation.Implementations.Reviews;
using System.Security.Claims;
using Entities.Enums;
using ServiceImplementation.Helpers;

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

        /// <summary>
        /// Retrieves the list of contracts associated with the logged-in user.
        /// </summary>
        /// <param name="status">Optional status to filter contracts.</param>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 10).</param>
        /// <returns>A paged list of contract details.</returns>
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
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Retrieves details of a specific contract by its ID.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <returns>The contract details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContractReadDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetContractById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetContractByIdQuery(id, userId));
            if (!result.Succeeded) return NotFound(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Accepts a contract offer.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <returns>A boolean indicating success.</returns>
        [HttpPost("{id}/accept-offer")]
        [ProducesResponseType(typeof(bool), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> AcceptOffer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new AcceptOfferCommand(id, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return StatusCode(201, result.Data);
        }

        /// <summary>
        /// Declines a contract offer.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <returns>No content on success.</returns>
        [HttpPost("{id}/decline-offer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> DeclineOffer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new DeclineOfferCommand(id, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return NoContent();
        }

        /// <summary>
        /// Revokes a pending contract offer.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <returns>No content on success.</returns>
        [HttpPost("{id}/revoke-offer")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> RevokeOffer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new RevokeOfferCommand(id, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return NoContent();
        }

        /// <summary>
        /// Delivers work for a contract.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <param name="note">Note describing the delivery.</param>
        /// <param name="files">List of uploaded files.</param>
        /// <returns>The created work delivery details.</returns>
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
            if (!result.Succeeded)
            {
                {
                    return BadRequest(result);
                }
            }
            return StatusCode(201, result.Data);
        }

        /// <summary>
        /// Downloads an attachment for a contract delivery.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <param name="deliveryId">The work delivery ID.</param>
        /// <param name="attachmentId">The attachment ID.</param>
        /// <returns>The requested file attachment.</returns>
        [HttpGet("{id}/deliveries/{deliveryId}/attachments/{attachmentId}/download")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DownloadAttachment(int id, int deliveryId, Guid attachmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new DownloadAttachmentQuery(id, deliveryId, attachmentId, userId));
            if (!result.Succeeded)
            {
                if (result.ErrorCode == ErrorCodes.Unauthorized) return Forbid();
                return NotFound(result);
            }

            return PhysicalFile(result.Data.PhysicalPath, result.Data.ContentType, result.Data.OriginalFileName);
        }

        /// <summary>
        /// Marks a contract as completed.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <returns>No content on success.</returns>
        [HttpPost("{id}/complete")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CompleteContract(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new MarkContractAsCompletedCommand(id, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return NoContent();
        }

        /// <summary>
        /// Rejects a contract delivery.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <returns>No content on success.</returns>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> RejectContract(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new RejectContractCommand(id, userId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return NoContent();
        }

        /// <summary>
        /// Submits a review for a completed contract.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <param name="dto">The review details.</param>
        /// <returns>The created contract review details.</returns>
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
            if (!result.Succeeded)
            {
                if (result.ErrorCode == ServiceImplementation.Helpers.ErrorCodes.AlreadyReviewed)
                {
                    return Conflict(new ProblemDetails
                    {
                        Status = 409,
                        Title = "Already Reviewed",
                        Detail = result.Message
                    });
                }
                return BadRequest(result);
            }
            return StatusCode(201, result.Data);
        }

        /// <summary>
        /// Creates a direct contract offer from a client to a freelancer.
        /// </summary>
        /// <param name="command">The details of the direct offer.</param>
        /// <returns>The created contract details.</returns>
        [HttpPost("create-offer")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(ContractDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateDirectOffer([FromBody] CreateDirectOfferCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Ensure the client creating the offer is the one authenticated
            command.ClientId = userId;

            var result = await _mediator.Send(command);
            if (result.Succeeded)
            {
                return StatusCode(201, result.Data);
            }
            return BadRequest(result.Errors);
        }
        [HttpGet("{id}/deliveries")]
        [ProducesResponseType(typeof(IEnumerable<ContractDeliveryDto>), 200)]
        public async Task<IActionResult> GetContractDeliveries(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Note: Currently no role-based restriction in command, but logic can be verified.
            var result = await _mediator.Send(new GetContractDeliveriesQuery(id));
            return Ok(result);
        }

        [HttpGet("{id}/escrow")]
        [ProducesResponseType(typeof(EscrowSummaryDto), 200)]
        public async Task<IActionResult> GetEscrowSummary(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetEscrowSummaryQuery(id));
            return Ok(result);
        }
    }
}
