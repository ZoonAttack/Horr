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

        /// <summary>
        /// Retrieves the list of conversations for the logged-in user.
        /// </summary>
        /// <returns>A list of conversations.</returns>
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

        /// <summary>
        /// Retrieves messages for a specific conversation.
        /// </summary>
        /// <param name="id">The conversation ID.</param>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 20).</param>
        /// <returns>A paged list of messages.</returns>
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

        /// <summary>
        /// Sends a message within a specific conversation.
        /// </summary>
        /// <param name="id">The conversation ID.</param>
        /// <param name="body">The text body of the message.</param>
        /// <param name="files">Optional list of files to attach.</param>
        /// <param name="jobPostId">Optional associated job post ID.</param>
        /// <param name="receiverId">Optional ID of the message receiver.</param>
        /// <returns>The sent message details.</returns>
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
