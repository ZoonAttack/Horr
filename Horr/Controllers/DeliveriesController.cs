using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Implementations.Contracts;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/deliveries")]
    public class DeliveriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DeliveriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public class SubmitDeliveryRequest
        {
            public int ContractId { get; set; }
            public Guid? ContractMilestoneId { get; set; }
            public string? DeliveryNote { get; set; }
            public List<AttachmentDto> Attachments { get; set; } = new();
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(typeof(ContractDeliveryDto), 201)]
        public async Task<IActionResult> Submit([FromBody] SubmitDeliveryRequest req)
        {
            var freelancerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new SubmitDeliveryCommand(
                req.ContractId,
                req.ContractMilestoneId,
                req.DeliveryNote,
                freelancerId,
                req.Attachments
            );
            var result = await _mediator.Send(command);
            return StatusCode(201, result);
        }

        [HttpPost("{deliveryId}/approve")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(ContractDeliveryDto), 200)]
        public async Task<IActionResult> Approve(Guid deliveryId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _mediator.Send(new ApproveDeliveryCommand(deliveryId, clientId));
            return Ok(result);
        }

        public class RequestRevisionRequest
        {
            public string Reason { get; set; } = string.Empty;
        }

        [HttpPost("{deliveryId}/revision")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(RevisionRequestDto), 201)]
        public async Task<IActionResult> RequestRevision(Guid deliveryId, [FromBody] RequestRevisionRequest req)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new RequestRevisionCommand(deliveryId, clientId, req.Reason);
            var result = await _mediator.Send(command);
            return StatusCode(201, result);
        }

        public class OpenDisputeRequest
        {
            public int ContractId { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        [HttpPost("{deliveryId}/dispute")]
        [Authorize(Roles = "Client, Freelancer")]
        [ProducesResponseType(typeof(DisputeDto), 201)]
        public async Task<IActionResult> OpenDispute(Guid deliveryId, [FromBody] OpenDisputeRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new OpenDisputeCommand(req.ContractId, deliveryId, userId, req.Reason);
            var result = await _mediator.Send(command);
            return StatusCode(201, result);
        }
    }
}
