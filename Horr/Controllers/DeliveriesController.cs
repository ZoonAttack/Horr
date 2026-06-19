using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.Storage;
using ServiceImplementation.Helpers;
using ServiceImplementation.Implementations.Contracts;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/deliveries")]
    public class DeliveriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorage;

        public DeliveriesController(IMediator mediator, IFileStorageService fileStorage)
        {
            _mediator = mediator;
            _fileStorage = fileStorage;
        }

        public class SubmitDeliveryRequest
        {
            public int ContractId { get; set; }
            public Guid? ContractMilestoneId { get; set; }
            public string? DeliveryNote { get; set; }
            public List<AttachmentDto> Attachments { get; set; } = new();
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Freelancer")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<AttachmentDto>), 200)]
        public async Task<IActionResult> Upload(List<IFormFile> files, CancellationToken ct)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var attachments = new List<AttachmentDto>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var storedFile = await _fileStorage.SaveAsync(file, "deliveries", ct);
                    attachments.Add(new AttachmentDto
                    {
                        Id = Guid.NewGuid(),
                        FileUrl = storedFile.FileUrl,
                        OriginalFileName = storedFile.OriginalFileName,
                        FileType = storedFile.FileType,
                        FileSizeBytes = storedFile.FileSizeBytes,
                        UploadedAt = DateTime.UtcNow,
                        FileName = storedFile.OriginalFileName,
                        StoragePath = storedFile.FileUrl
                    });
                }
            }

            return Ok(attachments);
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

            try
            {
                var result = await _mediator.Send(command);
                return StatusCode(201, result);
            }
            catch (Exception)
            {
                if (req.Attachments != null && req.Attachments.Count > 0)
                {
                    foreach (var attachment in req.Attachments)
                    {
                        if (!string.IsNullOrWhiteSpace(attachment.FileUrl))
                        {
                            try
                            {
                                _fileStorage.Delete(attachment.FileUrl);
                            }
                            catch
                            {
                                // Fail silent on delete error during exception handling
                            }
                        }
                    }
                }
                throw;
            }
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
        [ProducesResponseType(typeof(RevisionRequestDto), 200)]
        [ProducesResponseType(typeof(Result<RevisionRequestDto>), 400)]
        public async Task<IActionResult> RequestRevision(Guid deliveryId, [FromBody] RequestRevisionRequest req)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var command = new RequestRevisionCommand(deliveryId, clientId, req.Reason);
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return StatusCode(201, result.Data);
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

        [HttpGet("attachments/{attachmentId}/download")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DownloadAttachment(Guid attachmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new DownloadDeliveryAttachmentQuery(attachmentId, userId));
            if (!result.Succeeded)
            {
                if (result.ErrorCode == ErrorCodes.Unauthorized) return Forbid();
                return NotFound(result);
            }

            return PhysicalFile(result.Data.PhysicalPath, result.Data.ContentType, result.Data.OriginalFileName);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ContractDeliveryDto>), 200)]
        public async Task<IActionResult> GetContractDeliveries([FromQuery] int contractId)
        {
            var result = await _mediator.Send(new GetContractDeliveriesQuery(contractId));
            return Ok(result);
        }
    }
}
