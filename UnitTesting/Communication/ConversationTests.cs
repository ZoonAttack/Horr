using Entities;
using Entities.Communication;
using Entities.Enums;
using Entities.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTesting.Communication
{
    public class ConversationTests : IDisposable
    {
        private readonly AppDbContext _context;

        public ConversationTests()
        {
            _context = DbContextUtility.CreateSqliteDbContext(Guid.NewGuid().ToString());
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task ConversationParticipant_Should_Enforce_Composite_PK()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "user-1", FullName = "Test User", UserName = "testuser", Email = "test@test.com" };
            var conversation = new Conversation { Id = "conv-1" };
            _context.Users.Add(user);
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var participant1 = new ConversationParticipant { ConversationId = "conv-1", UserId = "user-1" };
            _context.ConversationParticipants.Add(participant1);
            await _context.SaveChangesAsync();

            // Detach to allow adding another instance with same key
            _context.Entry(participant1).State = EntityState.Detached;

            // Act
            var participant2 = new ConversationParticipant { ConversationId = "conv-1", UserId = "user-1" };
            _context.ConversationParticipants.Add(participant2);

            // Assert
            Func<Task> act = async () => await _context.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task Message_GlobalQueryFilter_Should_Exclude_Deleted_Messages()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "user-2", FullName = "Test User", UserName = "testuser2", Email = "test2@test.com" };
            var conversation = new Conversation { Id = "conv-2" };
            _context.Users.Add(user);
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var message1 = new Message { Id = "msg-1", ConversationId = "conv-2", SenderId = "user-2", Body = "Visible", IsDeleted = false };
            var message2 = new Message { Id = "msg-2", ConversationId = "conv-2", SenderId = "user-2", Body = "Hidden", IsDeleted = true };
            
            _context.Messages.AddRange(message1, message2);
            await _context.SaveChangesAsync();

            // Act
            var messages = await _context.Messages.ToListAsync();

            // Assert
            messages.Should().HaveCount(1);
            messages.Should().ContainSingle(m => m.Body == "Visible");
        }

        [Fact]
        public async Task Conversation_GlobalQueryFilter_Should_Exclude_Deleted_Conversations()
        {
            // Arrange
            var conv1 = new Conversation { Id = "conv-active", IsDeleted = false };
            var conv2 = new Conversation { Id = "conv-deleted", IsDeleted = true };

            _context.Conversations.AddRange(conv1, conv2);
            await _context.SaveChangesAsync();

            // Act
            var conversations = await _context.Conversations.ToListAsync();

            // Assert
            conversations.Should().HaveCount(1);
            conversations.Should().ContainSingle(c => c.Id == "conv-active");
        }

        [Fact]
        public async Task SoftDeletedConversation_Should_Hide_Its_Messages()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "user-3", FullName = "Test User", UserName = "testuser3", Email = "test3@test.com" };
            var conversation = new Conversation { Id = "conv-deleted-with-msgs", IsDeleted = true };
            _context.Users.Add(user);
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var message = new Message { Id = "msg-on-deleted-conv", ConversationId = "conv-deleted-with-msgs", SenderId = "user-3", Body = "Hidden" };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var messages = await _context.Messages.ToListAsync();

            // Assert
            // Since the Conversation is soft-deleted, it shouldn't be found via navigation or directly?
            // Actually, the Message filter only checks m.IsDeleted.
            // The requirement says: "When a Conversation is soft-deleted, the system shall hide all of its Messages from queries as a consequence of the Message Global Query Filter."
            // This implies I might need to update the Message filter to check if the Conversation is deleted too?
            // Or maybe the requirement implies that deleting a conversation SHOULD mark all its messages as deleted.
            // But if it's a "consequence of the Message Global Query Filter", it suggests the filter itself handles it.
            
            // Let's check the current filter: modelBuilder.Entity<Message>().HasQueryFilter(m => !m.IsDeleted);
            // This doesn't check Conversation.IsDeleted.
            
            // I should update the filter in AppDbContext.
            messages.Should().HaveCount(0);
        }
    }
}
