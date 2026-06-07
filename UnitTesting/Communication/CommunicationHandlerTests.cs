using Entities;
using Entities.Communication;
using Entities.Project;
using Entities.Enums;
using Entities.Users;
using FluentAssertions;
using ServiceImplementation.Hubs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Implementations.Communication;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Xunit;

namespace UnitTesting.Communication
{
    public class CommunicationHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IHubContext<ChatHub>> _mockHubContext;
        private readonly Mock<IHubClients> _mockClients;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;

        public CommunicationHandlerTests()
        {
            _context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            _context.Database.EnsureCreated();

            _mockHubContext = new Mock<IHubContext<ChatHub>>();
            _mockClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
            _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockWebHostEnvironment.Setup(w => w.WebRootPath).Returns(Directory.GetCurrentDirectory());
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedBaseData()
        {
            var user1 = new Entities.Users.User { Id = "user-1", FullName = "User 1", Email = "u1@test.com", UserName = "u1" };
            var user2 = new Entities.Users.User { Id = "user-2", FullName = "User 2", Email = "u2@test.com", UserName = "u2" };
            
            var client = new Client { UserId = "user-1" };
            var freelancer = new Freelancer { UserId = "user-2", Availability = "FullTime" };

            var contract = new Contract { Id = 1, ClientId = "user-1", FreelancerId = "user-2", Status = ContractStatus.Active };
            var chat = new Chat { Id = "conv-1", ContractId = 1, ClientId = "user-1", FreelancerId = "user-2" };

            _context.Users.AddRange(user1, user2);
            _context.Clients.Add(client);
            _context.Freelancers.Add(freelancer);
            _context.Contracts.Add(contract);
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
        }

        // 1. Assert SendTextMessage: persisted Message has Status = Unread
        [Fact]
        public async Task SendTextMessage_Should_Persist_With_Status_Unread()
        {
            // Arrange
            await SeedBaseData();
            var handler = new SendTextMessageCommandHandler(_context, _mockHubContext.Object);
            var command = new SendTextMessageCommand("conv-1", "user-1", "Hello World");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var message = await _context.Messages.FirstAsync(m => m.Id == result.Data.Id);
            message.Status.Should().Be(MessageStatus.Unread);
        }

        // 2. Assert UploadChatFile: Attachment row created with correct FileUrl and FileType values
        [Fact]
        public async Task UploadChatFile_Should_Create_Attachment()
        {
            // Arrange
            await SeedBaseData();
            var handler = new UploadChatFileCommandHandler(_context, _mockHubContext.Object, _mockWebHostEnvironment.Object);
            
            var file1 = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("test")), 0, 4, "file", "test1.jpg");
            var command = new UploadChatFileCommand("conv-1", "user-1", file1);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var attachments = await _context.Attachments.Where(a => a.MessageId == result.Data.Id).ToListAsync();
            attachments.Should().HaveCount(1);
            attachments[0].FileType.Should().Be(".jpg");
            attachments[0].FileUrl.Should().StartWith("/uploads/chat/");
        }

        // 3. Assert SendTextMessage: IHubContext.Clients.Group("conv-1").SendAsync("ReceiveMessage", ...) called exactly once
        [Fact]
        public async Task SendTextMessage_Should_Broadcast_Via_SignalR()
        {
            // Arrange
            await SeedBaseData();
            var handler = new SendTextMessageCommandHandler(_context, _mockHubContext.Object);
            var command = new SendTextMessageCommand("conv-1", "user-1", "Hello SignalR");

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            _mockClients.Verify(c => c.Group("conv-1"), Times.Once);
            _mockClientProxy.Verify(
                x => x.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(o => o.Length == 1 && ((MessageDto)o[0]).Body == "Hello SignalR"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // 4. Assert GetChatsByUser: seed 5 messages (3 Unread from other user, 2 Unread from current user) → UnreadCount = 3
        [Fact]
        public async Task GetChatsByUser_Should_Return_Correct_UnreadCount()
        {
            // Arrange
            await SeedBaseData();
            var otherUser = "user-2";
            var currentUser = "user-1";

            // 3 Unread from other user
            for (int i = 0; i < 3; i++)
                _context.Messages.Add(new Message { Id = $"msg-other-{i}", ChatId = "conv-1", SenderId = otherUser, Body = $"Msg {i}", Status = MessageStatus.Unread });
            
            // 2 Unread from current user
            for (int i = 0; i < 2; i++)
                _context.Messages.Add(new Message { Id = $"msg-my-{i}", ChatId = "conv-1", SenderId = currentUser, Body = $"My Msg {i}", Status = MessageStatus.Unread });

            await _context.SaveChangesAsync();

            var handler = new GetChatsByUserQueryHandler(_context);
            var query = new GetChatsByUserQuery(currentUser, UserRole.Client);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().ContainSingle(c => c.ChatId == "conv-1");
            result.Data.First(c => c.ChatId == "conv-1").UnreadCount.Should().Be(3);
        }

        // 5. Assert GetChatsByUser: seed message with 70-char body → LastMessagePreview is truncated to 60 chars
        [Fact]
        public async Task GetChatsByUser_Should_Truncate_LastMessagePreview()
        {
            // Arrange
            await SeedBaseData();
            var longBody = "This is a very long message body that exceeds sixty characters for testing purposes.";
            _context.Messages.Add(new Message { Id = "msg-long", ChatId = "conv-1", SenderId = "user-2", Body = longBody, SentAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var handler = new GetChatsByUserQueryHandler(_context);
            var query = new GetChatsByUserQuery("user-1", UserRole.Client);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var preview = result.Data.First(c => c.ChatId == "conv-1").LastMessagePreview;
            preview.Length.Should().Be(60);
            preview.Should().Be(longBody.Substring(0, 60));
        }

        // 6. Assert GetChatMessages: seed 4 Unread from other user + 1 Unread from current user → after fetch, 4 marked Read, current user's 1 still Unread
        [Fact]
        public async Task GetChatMessages_Should_Mark_Other_Users_Messages_As_Read()
        {
            // Arrange
            await SeedBaseData();
            var otherUser = "user-2";
            var currentUser = "user-1";

            for (int i = 0; i < 4; i++)
                _context.Messages.Add(new Message { Id = $"msg-o-{i}", ChatId = "conv-1", SenderId = otherUser, Body = $"Other Msg {i}", Status = MessageStatus.Unread, SentAt = DateTime.UtcNow.AddMinutes(i) });
            
            _context.Messages.Add(new Message { Id = "msg-m-1", ChatId = "conv-1", SenderId = currentUser, Body = "My Msg", Status = MessageStatus.Unread, SentAt = DateTime.UtcNow.AddMinutes(5) });
            
            await _context.SaveChangesAsync();

            var handler = new GetChatMessagesQueryHandler(_context);
            var query = new GetChatMessagesQuery("conv-1", currentUser);

            // Act
            await handler.Handle(query, CancellationToken.None);

            // Assert
            var otherMsgs = await _context.Messages.Where(m => m.SenderId == otherUser).ToListAsync();
            otherMsgs.All(m => m.Status == MessageStatus.Read).Should().BeTrue();

            var myMsg = await _context.Messages.FirstAsync(m => m.SenderId == currentUser);
            myMsg.Status.Should().Be(MessageStatus.Unread);
        }

        // 7. Assert GetChatMessages: results ordered newest-first — assert first item has the latest SentAt value
        [Fact]
        public async Task GetChatMessages_Should_Be_Ordered_Newest_First()
        {
            // Arrange
            await SeedBaseData();
            var now = DateTime.UtcNow;
            _context.Messages.Add(new Message { Id = "msg-old", ChatId = "conv-1", SenderId = "user-2", Body = "Old", SentAt = now.AddHours(-1) });
            _context.Messages.Add(new Message { Id = "msg-new", ChatId = "conv-1", SenderId = "user-2", Body = "New", SentAt = now });
            await _context.SaveChangesAsync();

            var handler = new GetChatMessagesQueryHandler(_context);
            var query = new GetChatMessagesQuery("conv-1", "user-1");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Items.First().Body.Should().Be("New");
            result.Data.Items.First().SentAt.Should().BeOnOrAfter(result.Data.Items.Last().SentAt);
        }

        // 8. Assert GetChatMessages: soft-deleted message does not appear in results
        [Fact]
        public async Task GetChatMessages_Should_Exclude_SoftDeleted_Messages()
        {
            // Arrange
            await SeedBaseData();
            _context.Messages.Add(new Message { Id = "msg-del", ChatId = "conv-1", SenderId = "user-2", Body = "Deleted", IsDeleted = true });
            _context.Messages.Add(new Message { Id = "msg-vis", ChatId = "conv-1", SenderId = "user-2", Body = "Visible", IsDeleted = false });
            await _context.SaveChangesAsync();

            var handler = new GetChatMessagesQueryHandler(_context);
            var query = new GetChatMessagesQuery("conv-1", "user-1");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Items.Should().HaveCount(1);
            result.Data.Items.Should().ContainSingle(m => m.Body == "Visible");
        }

        // 9. Assert GetChatMessages: unknown conversationId returns false Succeeded status
        [Fact]
        public async Task GetChatMessages_Should_Return_Failure_For_Unknown_Conversation()
        {
            // Arrange
            await SeedBaseData();
            var handler = new GetChatMessagesQueryHandler(_context);
            var query = new GetChatMessagesQuery("unknown-conv", "user-1");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);
 
            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCodes.Unauthorized);
        }
    }
}
