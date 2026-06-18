using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Implementations.Contracts;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/contracts/{contractId}/deliveries/{deliveryId}/specialist-review")]
    public class ContractSpecialistReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContractSpecialistReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> RequestReview(
            [FromRoute] int contractId,
            [FromRoute] Guid deliveryId,
            [FromBody] RequestSpecialistReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new RequestSpecialistReviewCommand(
                deliveryId,
                userId,
                dto.ReviewerType,
                dto.RequirementsSummary
            );

            var result = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetSpecialistReview),
                new { contractId, deliveryId },
                result
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecialistReview(
            [FromRoute] int contractId,
            [FromRoute] Guid deliveryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var query = new GetDeliverySpecialistReviewQuery(deliveryId, userId);

            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                return NotFound(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> SubmitSpecialistReview(
            [FromRoute] int contractId,
            [FromRoute] Guid deliveryId,
            [FromBody] SubmitSpecialistReviewDto dto)
        {
            var specialistId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var currentReviewResult = await _mediator.Send(new GetDeliverySpecialistReviewQuery(deliveryId, specialistId));
            if (!currentReviewResult.Succeeded || currentReviewResult.Data == null)
            {
                return NotFound(new { message = "Active review not found for this delivery." });
            }

            var command = new SubmitHumanSpecialistReviewCommand(
                currentReviewResult.Data.Id,
                specialistId,
                dto.Verdict,
                dto.ReviewNote
            );

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
