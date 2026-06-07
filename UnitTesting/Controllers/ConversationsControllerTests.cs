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
using Xunit;
using Entities.Enums;

namespace UnitTesting.Controllers
{
    public class ConversationsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ConversationsController _controller;
        private const string CurrentUserId = "test-user-id";

        public ConversationsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ConversationsController(_mediatorMock.Object);

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
        public async Task GetConversations_ShouldReturnOk_WithConversations()
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
            var result = await _controller.GetConversations(UserRole.Client);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetConversations_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _controller.GetConversations(UserRole.Client);

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
                ErrorCode = "CONVERSATION_NOT_FOUND",
                Message = "Conversation not found."
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetChatMessagesQuery>(q => q.ChatId == conversationId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(resultData);

            // Act
            var result = await _controller.GetMessages(conversationId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var problem = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(404);
            problem.Title.Should().Be("Conversation Not Found");
        }

        [Fact]
        public async Task SendMessage_ShouldReturnCreated_WithMessageDto_WhenConversationExists()
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
            var result = await _controller.SendMessage(conversationId, body, null);

            // Assert
            var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(responseDto);
        }

        [Fact]
        public async Task SendMessage_ShouldReturnBadRequest_WhenBodyIsEmpty()
        {
            // Act
            var result = await _controller.SendMessage("conv-1", " ", null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
