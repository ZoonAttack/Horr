using Entities;
using Entities.Communication;
using Entities.Enums;
using Entities.Users;
using FluentAssertions;
using ServiceImplementation.Hubs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Implementations.Communication;
using ServiceContracts.DTOs.Responses;
using System.Text;

namespace UnitTesting.Communication
{
    public class CommunicationHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IHubContext<ChatHub>> _mockHubContext;
        private readonly Mock<IHubClients> _mockClients;
        private readonly Mock<IClientProxy> _mockClientProxy;

        public CommunicationHandlerTests()
        {
            _context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            _context.Database.EnsureCreated();

            _mockHubContext = new Mock<IHubContext<ChatHub>>();
            _mockClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
            _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
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
            var conv = new Conversation { Id = "conv-1" };
            var p1 = new ConversationParticipant { ConversationId = "conv-1", UserId = "user-1" };
            var p2 = new ConversationParticipant { ConversationId = "conv-1", UserId = "user-2" };

            _context.Users.AddRange(user1, user2);
            _context.Conversations.Add(conv);
            _context.ConversationParticipants.AddRange(p1, p2);
            await _context.SaveChangesAsync();
        }

        // 1. Assert SendMessage: persisted Message has Status = Unread
        [Fact]
        public async Task SendMessage_Should_Persist_With_Status_Unread()
        {
            // Arrange
            await SeedBaseData();
            var handler = new SendMessageCommandHandler(_context, _mockHubContext.Object);
            var command = new SendMessageCommand("conv-1", "user-1", "Hello World");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var message = await _context.Messages.FirstAsync(m => m.Id == result.Data.Id);
            message.Status.Should().Be(MessageStatus.Unread);
        }

        // 2. Assert SendMessage with 2 uploaded files: exactly 2 Attachment rows created with correct FileUrl and FileType values
        [Fact]
        public async Task SendMessage_With_Files_Should_Create_Attachments()
        {
            // Arrange
            await SeedBaseData();
            var handler = new SendMessageCommandHandler(_context, _mockHubContext.Object);
            
            var file1 = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("test")), 0, 4, "files", "test1.jpg");
            var file2 = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("test2")), 0, 5, "files", "test2.pdf");
            
            var command = new SendMessageCommand("conv-1", "user-1", "Hello with files", new List<IFormFile> { file1, file2 });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var attachments = await _context.Attachments.Where(a => a.MessageId == result.Data.Id).ToListAsync();
            attachments.Should().HaveCount(2);
            attachments.Should().ContainSingle(a => a.FileType == ".jpg");
            attachments.Should().ContainSingle(a => a.FileType == ".pdf");
            attachments.All(a => a.FileUrl.StartsWith("/uploads/chat/")).Should().BeTrue();
        }

        // 3. Assert SendMessage: IHubContext.Clients.Group("conv-123").SendAsync("ReceiveMessage", ...) called exactly once (verify via Moq)
        [Fact]
        public async Task SendMessage_Should_Broadcast_Via_SignalR()
        {
            // Arrange
            await SeedBaseData();
            var handler = new SendMessageCommandHandler(_context, _mockHubContext.Object);
            var command = new SendMessageCommand("conv-1", "user-1", "Hello SignalR");

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

        // 4. Assert GetConversations: seed 5 messages (3 Unread from other user, 2 Unread from current user) → UnreadCount = 3
        [Fact]
        public async Task GetConversations_Should_Return_Correct_UnreadCount()
        {
            // Arrange
            await SeedBaseData();
            var otherUser = "user-2";
            var currentUser = "user-1";

            // 3 Unread from other user
            for (int i = 0; i < 3; i++)
                _context.Messages.Add(new Message { Id = $"msg-other-{i}", ConversationId = "conv-1", SenderId = otherUser, Body = $"Msg {i}", Status = MessageStatus.Unread });
            
            // 2 Unread from current user
            for (int i = 0; i < 2; i++)
                _context.Messages.Add(new Message { Id = $"msg-my-{i}", ConversationId = "conv-1", SenderId = currentUser, Body = $"My Msg {i}", Status = MessageStatus.Unread });

            await _context.SaveChangesAsync();

            var handler = new GetConversationsQueryHandler(_context);
            var query = new GetConversationsQuery(currentUser);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().ContainSingle(c => c.Id == "conv-1");
            result.Data.First(c => c.Id == "conv-1").UnreadCount.Should().Be(3);
        }

        // 5. Assert GetConversations: seed message with 60-char body → LastMessagePreview ends with "..." and total length = 53
        [Fact]
        public async Task GetConversations_Should_Truncate_LastMessagePreview()
        {
            // Arrange
            await SeedBaseData();
            var longBody = "This is a very long message body that exceeds fifty characters for testing.";
            _context.Messages.Add(new Message { Id = "msg-long", ConversationId = "conv-1", SenderId = "user-2", Body = longBody, SentAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var handler = new GetConversationsQueryHandler(_context);
            var query = new GetConversationsQuery("user-1");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var preview = result.Data.First(c => c.Id == "conv-1").LastMessagePreview;
            preview.Should().EndWith("...");
            preview.Length.Should().Be(53);
        }

        // 6. Assert GetMessages: seed 4 Unread from other user + 1 Unread from current user → after fetch, 4 marked Read, current user's 1 still Unread
        [Fact]
        public async Task GetMessages_Should_Mark_Other_Users_Messages_As_Read()
        {
            // Arrange
            await SeedBaseData();
            var otherUser = "user-2";
            var currentUser = "user-1";

            for (int i = 0; i < 4; i++)
                _context.Messages.Add(new Message { Id = $"msg-o-{i}", ConversationId = "conv-1", SenderId = otherUser, Body = $"Other Msg {i}", Status = MessageStatus.Unread, SentAt = DateTime.UtcNow.AddMinutes(i) });
            
            _context.Messages.Add(new Message { Id = "msg-m-1", ConversationId = "conv-1", SenderId = currentUser, Body = "My Msg", Status = MessageStatus.Unread, SentAt = DateTime.UtcNow.AddMinutes(5) });
            
            await _context.SaveChangesAsync();

            var handler = new GetMessagesQueryHandler(_context);
            var query = new GetMessagesQuery("conv-1", currentUser);

            // Act
            await handler.Handle(query, CancellationToken.None);

            // Assert
            var otherMsgs = await _context.Messages.Where(m => m.SenderId == otherUser).ToListAsync();
            otherMsgs.All(m => m.Status == MessageStatus.Read).Should().BeTrue();

            var myMsg = await _context.Messages.FirstAsync(m => m.SenderId == currentUser);
            myMsg.Status.Should().Be(MessageStatus.Unread);
        }

        // 7. Assert GetMessages: results ordered newest-first — assert first item has the latest SentAt value
        [Fact]
        public async Task GetMessages_Should_Be_Ordered_Newest_First()
        {
            // Arrange
            await SeedBaseData();
            var now = DateTime.UtcNow;
            _context.Messages.Add(new Message { Id = "msg-old", ConversationId = "conv-1", SenderId = "user-2", Body = "Old", SentAt = now.AddHours(-1) });
            _context.Messages.Add(new Message { Id = "msg-new", ConversationId = "conv-1", SenderId = "user-2", Body = "New", SentAt = now });
            await _context.SaveChangesAsync();

            var handler = new GetMessagesQueryHandler(_context);
            var query = new GetMessagesQuery("conv-1", "user-1");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Items.First().Body.Should().Be("New");
            result.Data.Items.First().SentAt.Should().BeOnOrAfter(result.Data.Items.Last().SentAt);
        }

        // 8. Assert GetMessages: soft-deleted message does not appear in results
        [Fact]
        public async Task GetMessages_Should_Exclude_SoftDeleted_Messages()
        {
            // Arrange
            await SeedBaseData();
            _context.Messages.Add(new Message { Id = "msg-del", ConversationId = "conv-1", SenderId = "user-2", Body = "Deleted", IsDeleted = true });
            _context.Messages.Add(new Message { Id = "msg-vis", ConversationId = "conv-1", SenderId = "user-2", Body = "Visible", IsDeleted = false });
            await _context.SaveChangesAsync();

            var handler = new GetMessagesQueryHandler(_context);
            var query = new GetMessagesQuery("conv-1", "user-1");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Items.Should().HaveCount(1);
            result.Data.Items.Should().ContainSingle(m => m.Body == "Visible");
        }

        // 9. Assert GetMessages: unknown conversationId throws NotFoundException
        [Fact]
        public async Task GetMessages_Should_Throw_NotFound_For_Unknown_Conversation()
        {
            // Arrange
            await SeedBaseData();
            var handler = new GetMessagesQueryHandler(_context);
            var query = new GetMessagesQuery("unknown-conv", "user-1");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);
 
            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be("CONVERSATION_NOT_FOUND");
        }

        private async Task SeedJobData()
        {
            var category = new Entities.Project.Category { Id = "cat-1", Name = "Test Category", Slug = "test-category" };
            var job = new Entities.Project.JobPost 
            { 
                Id = "job-123", 
                Title = "Test Job", 
                Description = "Description", 
                CategoryId = "cat-1", 
                ClientId = "user-1" 
            };
            _context.Categories.Add(category);
            _context.JobPosts.Add(job);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task SendMessage_Should_InitiateNewConversation_WhenItDoesNotExist_AndFormDataProvided()
        {
            // Arrange
            await SeedBaseData();
            await SeedJobData();
            var handler = new SendMessageCommandHandler(_context, _mockHubContext.Object);
            
            var command = new SendMessageCommand(
                "new-conv-id", 
                "user-1", 
                "Initial message regarding job", 
                null, 
                "job-123", 
                "user-2"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Body.Should().Be("Initial message regarding job");

            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.Id == "new-conv-id");
            conversation.Should().NotBeNull();
            conversation.JobPostId.Should().Be("job-123");

            var participants = await _context.ConversationParticipants.Where(p => p.ConversationId == "new-conv-id").ToListAsync();
            participants.Should().HaveCount(2);
            participants.Should().ContainSingle(p => p.UserId == "user-1");
            participants.Should().ContainSingle(p => p.UserId == "user-2");
        }

        [Fact]
        public async Task SendMessage_Should_FailToInitiate_WhenItDoesNotExist_AndFormDataMissing()
        {
            // Arrange
            await SeedBaseData();
            var handler = new SendMessageCommandHandler(_context, _mockHubContext.Object);
            
            var command = new SendMessageCommand(
                "new-conv-id", 
                "user-1", 
                "Initial message", 
                null, 
                null, 
                null
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be("CONVERSATION_NOT_FOUND");
        }

        [Fact]
        public async Task SendMessage_Should_FailToInitiate_WhenJobPostDoesNotExist()
        {
            // Arrange
            await SeedBaseData();
            var handler = new SendMessageCommandHandler(_context, _mockHubContext.Object);
            
            var command = new SendMessageCommand(
                "new-conv-id", 
                "user-1", 
                "Initial message", 
                null, 
                "non-existent-job", 
                "user-2"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be("JOB_NOT_FOUND");
        }

        [Fact]
        public async Task SendMessage_Should_ReuseExistingConversation_WhenInviteToJobPairAlreadyExists()
        {
            // Arrange
            await SeedBaseData();
            await SeedJobData();
            var handler = new SendMessageCommandHandler(_context, _mockHubContext.Object);
            
            // First, create the initial conversation for the job post
            var command1 = new SendMessageCommand(
                "new-conv-id", 
                "user-1", 
                "Initial invitation message regarding job", 
                null, 
                "job-123", 
                "user-2"
            );
            var result1 = await handler.Handle(command1, CancellationToken.None);
            result1.Succeeded.Should().BeTrue();

            // Act - attempt to send a new invite to the same freelancer for the same job with a different/new ConversationId
            var command2 = new SendMessageCommand(
                "another-temp-conv-id", 
                "user-1", 
                "Follow up invitation message", 
                null, 
                "job-123", 
                "user-2"
            );
            var result2 = await handler.Handle(command2, CancellationToken.None);

            // Assert
            result2.Succeeded.Should().BeTrue();
            // It should have resolved to the existing conversation ID: "new-conv-id"
            result2.Data.ConversationId.Should().Be("new-conv-id");

            // Verify that only one conversation actually exists for this job post
            var conversationsCount = await _context.Conversations.CountAsync(c => c.JobPostId == "job-123");
            conversationsCount.Should().Be(1);

            // Verify both messages are in "new-conv-id"
            var messages = await _context.Messages.Where(m => m.ConversationId == "new-conv-id").ToListAsync();
            messages.Should().HaveCount(2);
            messages.Should().ContainSingle(m => m.Body == "Initial invitation message regarding job");
            messages.Should().ContainSingle(m => m.Body == "Follow up invitation message");
        }
    }
}
