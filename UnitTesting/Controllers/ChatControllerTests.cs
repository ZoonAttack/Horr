using FluentAssertions;
using Horr.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts;
using Services;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Implementations.Communication;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ServiceImplementation.Helpers;
using Xunit;
using Entities.Enums;

namespace UnitTesting.Controllers
{
    public class ChatControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ChatController _controller;
        private const string CurrentUserId = "test-user-id";

        public ChatControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ChatController(_mediatorMock.Object);

            // Mock User Identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, CurrentUserId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetChats_ShouldReturnOk_WithConversations()
        {
            // Arrange
            var list = new List<ChatSummaryDto>
            {
                new ChatSummaryDto { ChatId = "conv-1" }
            };
            var resultData = new Result<List<ChatSummaryDto>>
            {
                Succeeded = true,
                Data = list
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetChatsByUserQuery>(q => q.UserId == CurrentUserId && q.Role == UserRole.Client), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.GetChats(UserRole.Client);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetChats_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _controller.GetChats(UserRole.Client);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetMessages_ShouldReturnOk_WithPagedMessages()
        {
            // Arrange
            var conversationId = "conv-1";
            var pagedResult = new PagedResult<MessageDto>
            {
                Items = new List<MessageDto> { new MessageDto { Id = "msg-1", Body = "Hello", ChatId = conversationId } },
                TotalCount = 1,
                Page = 1,
                PageSize = 30
            };
            var resultData = new Result<PagedResult<MessageDto>>
            {
                Succeeded = true,
                Data = pagedResult
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetChatMessagesQuery>(q => q.ChatId == conversationId && q.UserId == CurrentUserId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.GetMessages(conversationId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(pagedResult);
        }

        [Fact]
        public async Task GetMessages_ShouldReturnNotFound_WhenConversationDoesNotExist()
        {
            // Arrange
            var conversationId = "non-existent";
            var resultData = new Result<PagedResult<MessageDto>>
            {
                Succeeded = false,
                ErrorCode = ErrorCodes.ChatNotFound,
                Message = "Conversation not found."
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetChatMessagesQuery>(q => q.ChatId == conversationId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.GetMessages(conversationId);

            // Assert
            var notFoundResult = result.Should().BeOfType<ObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);
            var problem = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(404);
            problem.Title.Should().Be("Not Found");
        }

        [Fact]
        public async Task SendTextMessage_ShouldReturnCreated_WithMessageDto_WhenConversationExists()
        {
            // Arrange
            var conversationId = "conv-1";
            var body = "Hello";
            var responseDto = new MessageDto { Id = "msg-1", ChatId = conversationId, Body = body };
            var resultData = new Result<MessageDto> { Succeeded = true, Data = responseDto };

            _mediatorMock.Setup(m => m.Send(It.Is<SendTextMessageCommand>(c => 
                c.ChatId == conversationId && 
                c.SenderId == CurrentUserId && 
                c.Text == body), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.SendTextMessage(conversationId, new SendTextMessageRequest { Text = body });

            // Assert
            var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(responseDto);
        }

        [Fact]
        public async Task SendTextMessage_ShouldReturnBadRequest_WhenBodyIsEmpty()
        {
            // Act
            var result = await _controller.SendTextMessage("conv-1", new SendTextMessageRequest { Text = " " });

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            var problem = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(400);
            problem.Title.Should().Be("Invalid Request");
        }

        [Fact]
        public async Task GetChatByContract_ShouldReturnOk_WithChatSummary_WhenExists()
        {
            // Arrange
            var contractId = 42;
            var summary = new ChatSummaryDto { ChatId = "conv-1", ContractId = contractId };
            var resultData = new Result<ChatSummaryDto> { Succeeded = true, Data = summary };

            _mediatorMock.Setup(m => m.Send(It.Is<GetChatByContractQuery>(q => q.ContractId == contractId && q.UserId == CurrentUserId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.GetChatByContract(contractId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(summary);
        }

        [Fact]
        public async Task GetChatByContract_ShouldReturnForbidden_WhenUnauthorized()
        {
            // Arrange
            var contractId = 42;
            var resultData = new Result<ChatSummaryDto>
            {
                Succeeded = false,
                ErrorCode = ServiceImplementation.Helpers.ErrorCodes.Unauthorized,
                Message = "Unauthorized access."
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetChatByContractQuery>(q => q.ContractId == contractId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.GetChatByContract(contractId);

            // Assert
            var forbiddenResult = result.Should().BeOfType<ObjectResult>().Subject;
            forbiddenResult.StatusCode.Should().Be(403);
            var problem = forbiddenResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(403);
            problem.Title.Should().Be("Forbidden");
        }

        [Fact]
        public async Task GetChatByContract_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            var contractId = 42;
            var resultData = new Result<ChatSummaryDto>
            {
                Succeeded = false,
                ErrorCode = ErrorCodes.ChatNotFound,
                Message = "Not found."
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetChatByContractQuery>(q => q.ContractId == contractId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.GetChatByContract(contractId);

            // Assert
            var notFoundResult = result.Should().BeOfType<ObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);
            var problem = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(404);
            problem.Title.Should().Be("Not Found");
        }

        [Fact]
        public async Task InitiateChat_ShouldReturnOk_WithChatSummary_WhenInitiationSucceeds()
        {
            // Arrange
            var contractId = 42;
            var summary = new ChatSummaryDto { ChatId = "conv-1", ContractId = contractId };
            var resultData = new Result<ChatSummaryDto> { Succeeded = true, Data = summary };

            _mediatorMock.Setup(m => m.Send(It.Is<CreateChatCommand>(c => c.ContractId == contractId && c.ClientId == CurrentUserId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.InitiateChat(new InitiateChatRequest { ContractId = contractId });

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(summary);
        }

        [Fact]
        public async Task InitiateChat_ShouldReturnForbidden_WhenClientNotAuthorized()
        {
            // Arrange
            var contractId = 42;
            var resultData = new Result<ChatSummaryDto>
            {
                Succeeded = false,
                ErrorCode = ServiceImplementation.Helpers.ErrorCodes.Unauthorized,
                Message = "Unauthorized client."
            };

            _mediatorMock.Setup(m => m.Send(It.Is<CreateChatCommand>(c => c.ContractId == contractId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.InitiateChat(new InitiateChatRequest { ContractId = contractId });

            // Assert
            var forbiddenResult = result.Should().BeOfType<ObjectResult>().Subject;
            forbiddenResult.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task InitiateChat_ShouldReturnNotFound_WhenContractDoesNotExist()
        {
            // Arrange
            var contractId = 42;
            var resultData = new Result<ChatSummaryDto>
            {
                Succeeded = false,
                ErrorCode = ServiceImplementation.Helpers.ErrorCodes.ContractNotFound,
                Message = "Contract not found."
            };

            _mediatorMock.Setup(m => m.Send(It.Is<CreateChatCommand>(c => c.ContractId == contractId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.InitiateChat(new InitiateChatRequest { ContractId = contractId });

            // Assert
            var notFoundResult = result.Should().BeOfType<ObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);
        }
    }
}
