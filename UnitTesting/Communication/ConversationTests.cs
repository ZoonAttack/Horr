using Entities;
using Entities.Communication;
using Entities.Project;
using Entities.Enums;
using Entities.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _context.Users.Add(user);

            var clientProfile = new Client { UserId = "user-2" };
            var freelancerProfile = new Freelancer { UserId = "user-2", Availability = "Full-Time" };
            _context.Clients.Add(clientProfile);
            _context.Freelancers.Add(freelancerProfile);

            var contract = new Contract { Id = 2, ClientId = "user-2", FreelancerId = "user-2", Status = ContractStatus.Active };
            _context.Contracts.Add(contract);

            var chat = new Chat { Id = "conv-2", ContractId = 2, ClientId = "user-2", FreelancerId = "user-2" };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            var message1 = new Message { Id = "msg-1", ChatId = "conv-2", SenderId = "user-2", Body = "Visible", IsDeleted = false };
            var message2 = new Message { Id = "msg-2", ChatId = "conv-2", SenderId = "user-2", Body = "Hidden", IsDeleted = true };
            
            _context.Messages.AddRange(message1, message2);
            await _context.SaveChangesAsync();

            // Act
            var messages = await _context.Messages.ToListAsync();

            // Assert
            messages.Should().HaveCount(1);
            messages.Should().ContainSingle(m => m.Body == "Visible");
        }

        [Fact]
        public async Task Chat_GlobalQueryFilter_Should_Exclude_Deleted_Chats()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "user-active", FullName = "Test User", UserName = "testuseractive", Email = "active@test.com" };
            _context.Users.Add(user);

            var clientProfile = new Client { UserId = "user-active" };
            var freelancerProfile = new Freelancer { UserId = "user-active", Availability = "Full-Time" };
            _context.Clients.Add(clientProfile);
            _context.Freelancers.Add(freelancerProfile);

            var contract1 = new Contract { Id = 10, ClientId = "user-active", FreelancerId = "user-active", Status = ContractStatus.Active };
            var contract2 = new Contract { Id = 11, ClientId = "user-active", FreelancerId = "user-active", Status = ContractStatus.Active };
            _context.Contracts.AddRange(contract1, contract2);

            var chat1 = new Chat { Id = "conv-active", ContractId = 10, ClientId = "user-active", FreelancerId = "user-active", IsDeleted = false };
            var chat2 = new Chat { Id = "conv-deleted", ContractId = 11, ClientId = "user-active", FreelancerId = "user-active", IsDeleted = true };

            _context.Chats.AddRange(chat1, chat2);
            await _context.SaveChangesAsync();

            // Act
            var chats = await _context.Chats.ToListAsync();

            // Assert
            chats.Should().HaveCount(1);
            chats.Should().ContainSingle(c => c.Id == "conv-active");
        }

        [Fact]
        public async Task SoftDeletedChat_Should_Hide_Its_Messages()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "user-3", FullName = "Test User", UserName = "testuser3", Email = "test3@test.com" };
            _context.Users.Add(user);

            var clientProfile = new Client { UserId = "user-3" };
            var freelancerProfile = new Freelancer { UserId = "user-3", Availability = "Full-Time" };
            _context.Clients.Add(clientProfile);
            _context.Freelancers.Add(freelancerProfile);

            var contract = new Contract { Id = 3, ClientId = "user-3", FreelancerId = "user-3", Status = ContractStatus.Active };
            _context.Contracts.Add(contract);

            var chat = new Chat { Id = "conv-deleted-with-msgs", ContractId = 3, ClientId = "user-3", FreelancerId = "user-3", IsDeleted = true };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            var message = new Message { Id = "msg-on-deleted-conv", ChatId = "conv-deleted-with-msgs", SenderId = "user-3", Body = "Hidden" };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var messages = await _context.Messages.ToListAsync();

            // Assert
            messages.Should().HaveCount(0);
        }
    }
}
