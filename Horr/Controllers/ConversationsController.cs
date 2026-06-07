using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Implementations.Communication;
using System.Security.Claims;
using Services;
using Entities.Enums;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceImplementation.Helpers;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConversationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ChatSummaryDto>), 200)]
        public async Task<IActionResult> GetConversations([FromQuery] UserRole role = UserRole.Client)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetChatsByUserQuery(userId, role));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        [HttpGet("{id}/messages")]
        [ProducesResponseType(typeof(PagedResult<MessageDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessages(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetChatMessagesQuery(id, userId, page, pageSize));
            if (!result.Succeeded)
            {
                if (result.ErrorCode == ErrorCodes.Unauthorized || result.ErrorCode == "CONVERSATION_NOT_FOUND")
                {
                    return NotFound(new ProblemDetails
                    {
                        Status = 404,
                        Title = "Conversation Not Found",
                        Detail = result.Message
                    });
                }
                return BadRequest(result);
            }
            return Ok(result.Data);
        }

        [HttpPost("{id}/messages")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(MessageDto), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendMessage(
            string id, 
            [FromForm] string? body, 
            [FromForm] List<IFormFile>? files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Validate that we have either a body or a file
            if (string.IsNullOrWhiteSpace(body) && (files == null || files.Count == 0))
            {
                return BadRequest("Message body or file is required.");
            }

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    long limit = 0;
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".webp")
                    {
                        limit = 10 * 1024 * 1024; // 10MB
                    }
                    else if (ext == ".mp4" || ext == ".mov" || ext == ".avi" || ext == ".webm")
                    {
                        limit = 150 * 1024 * 1024; // 150MB
                    }
                    else if (ext == ".pdf")
                    {
                        limit = 20 * 1024 * 1024; // 20MB
                    }
                    else
                    {
                        return BadRequest(new ProblemDetails
                        {
                            Status = 400,
                            Title = "Invalid File Extension",
                            Detail = $"File extension {ext} is not allowed."
                        });
                    }

                    if (file.Length > limit)
                    {
                        return BadRequest(new ProblemDetails
                        {
                            Status = 400,
                            Title = "File Too Large",
                            Detail = $"File {file.FileName} exceeds the allowed limit of {limit / (1024 * 1024)}MB."
                        });
                    }
                }

                // Call the upload command for the first file
                var result = await _mediator.Send(new UploadChatFileCommand(id, userId, files[0]));
                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }
                return StatusCode(201, result.Data);
            }
            else
            {
                // Call text message command
                var result = await _mediator.Send(new SendTextMessageCommand(id, userId, body!));
                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }
                return StatusCode(201, result.Data);
            }
        }
    }
}
