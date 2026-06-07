using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Implementations.Communication;
using System.Security.Claims;
using Services;

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
        [ProducesResponseType(typeof(IEnumerable<ConversationDto>), 200)]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetConversationsQuery(userId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        [HttpGet("{id}/messages")]
        [ProducesResponseType(typeof(PagedResult<MessageDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessages(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMessagesQuery(id, userId, page, pageSize));
            if (!result.Succeeded)
            {
                if (result.ErrorCode == "CONVERSATION_NOT_FOUND")
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
            [FromForm] string body, 
            [FromForm] List<IFormFile>? files,
            [FromForm] string? jobPostId = null,
            [FromForm] string? receiverId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(body))
            {
                return BadRequest("Message body is required.");
            }

            var result = await _mediator.Send(new SendMessageCommand(id, userId, body, files, jobPostId, receiverId));
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return StatusCode(201, result.Data);
        }
    }
}
