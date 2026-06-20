using Horr.Extentions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Implementations.Communication;
using Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceImplementation.Helpers;
using Services;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("initiate")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(ChatSummaryDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> InitiateChat([FromBody] InitiateChatRequest request)
        {
            string clientId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _mediator.Send(new CreateChatCommand(request.ContractId, clientId));
            if (!result.Succeeded)
            {
                int statusCode = 400;
                string title = "Bad Request";

                if (result.ErrorCode == ServiceImplementation.Helpers.ErrorCodes.Unauthorized)
                {
                    statusCode = 403;
                    title = "Forbidden";
                    
                    
                }
                else if (result.ErrorCode == ServiceImplementation.Helpers.ErrorCodes.ContractNotFound)
                {
                    statusCode = 404;
                    title = "Not Found";
                }

                return StatusCode(statusCode, new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ChatSummaryDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetChats([FromQuery] UserRole role = UserRole.Client)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetChatsByUserQuery(userId, role));
            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Failed to fetch chats",
                    Detail = result.Message
                });
            }
            return Ok(result.Data);
        }

        [HttpGet("by-contract/{contractId}")]
        [ProducesResponseType(typeof(ChatSummaryDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetChatByContract(int contractId)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetChatByContractQuery(contractId, userId));
            if (!result.Succeeded)
            {
                int statusCode = 400;
                string title = "Bad Request";

                if (result.ErrorCode == ServiceImplementation.Helpers.ErrorCodes.Unauthorized)
                {
                    statusCode = 403;
                    title = "Forbidden";
                }
                else if (result.ErrorCode == ErrorCodes.ChatNotFound)
                {
                    statusCode = 404;
                    title = "Not Found";
                }

                return StatusCode(statusCode, new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = result.Message
                });
            }
            return Ok(result.Data);
        }


        [HttpGet("{chatId}/messages")]
        [ProducesResponseType(typeof(PagedResult<MessageDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetMessages(string chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetChatMessagesQuery(chatId, userId, page, pageSize));
            if (!result.Succeeded)
            {
                int statusCode = 400;
                string title = "Bad Request";

                if (result.ErrorCode == ServiceImplementation.Helpers.ErrorCodes.Unauthorized)
                {
                    statusCode = 403;
                    title = "Forbidden";
                }
                else if (result.ErrorCode == ErrorCodes.ChatNotFound)
                {
                    statusCode = 404;
                    title = "Not Found";
                }

                return StatusCode(statusCode, new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = result.Message
                });
            }
            return Ok(result.Data);
        }

        [HttpPost("{chatId}/messages/text")]
        [ProducesResponseType(typeof(MessageDto), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> SendTextMessage(string chatId, [FromBody] SendTextMessageRequest request)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request?.Text))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Invalid Request",
                    Detail = "Message text is required."
                });
            }

            var result = await _mediator.Send(new SendTextMessageCommand(chatId, userId, request.Text));
            if (!result.Succeeded)
            {
                int statusCode = 400;
                string title = "Bad Request";

                if (result.ErrorCode == ServiceImplementation.Helpers.ErrorCodes.Unauthorized)
                {
                    statusCode = 403;
                    title = "Forbidden";
                }
                else if (result.ErrorCode == ErrorCodes.ChatNotFound)
                {
                    statusCode = 404;
                    title = "Not Found";
                }

                return StatusCode(statusCode, new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = result.Message
                });
            }
            return StatusCode(201, result.Data);
        }

        [HttpPost("{chatId}/messages/file")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(MessageDto), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> SendFileMessage(string chatId, IFormFile file)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Invalid Request",
                    Detail = "File is required."
                });
            }

            var result = await _mediator.Send(new UploadChatFileCommand(chatId, userId, file));
            if (!result.Succeeded)
            {
                int statusCode = 400;
                string title = "Bad Request";

                if (result.ErrorCode == ServiceImplementation.Helpers.ErrorCodes.Unauthorized)
                {
                    statusCode = 403;
                    title = "Forbidden";
                }
                else if (result.ErrorCode == ErrorCodes.ChatNotFound)
                {
                    statusCode = 404;
                    title = "Not Found";
                }
                else if (result.ErrorCode == ErrorCodes.InvalidFileType || result.ErrorCode == ErrorCodes.FileTooLarge)
                {
                    statusCode = 400;
                    title = "Invalid File";
                }

                return StatusCode(statusCode, new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = result.Message
                });
            }
            return StatusCode(201, result.Data);
        }
    }

    public class SendTextMessageRequest
    {
        public string Text { get; set; } = string.Empty;
    }

    public class InitiateChatRequest
    {
        public int ContractId { get; set; }
    }
}
