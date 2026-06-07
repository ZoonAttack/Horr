using Entities;
using Entities.Users;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceImplementation.Hubs;
using ServiceImplementation.Implementations.Communication;
using ServiceImplementation.Implementations.JobManagement;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTesting.User
{
    public class AccountDeletionTests : IDisposable
    {
        private readonly AppDbContext _context;

        public AccountDeletionTests()
        {
            _context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task SendMessage_Should_Return_AccountDeleted_When_User_Is_Deleted()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "deleted-user", FullName = "Deleted User", IsDeleted = true };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var mockHubContext = new Mock<IHubContext<ChatHub>>();
            var mockWebHost = new Mock<IWebHostEnvironment>();
            mockWebHost.Setup(w => w.WebRootPath).Returns(System.IO.Directory.GetCurrentDirectory());
            var handler = new SendMessageCommandHandler(_context, mockHubContext.Object, mockWebHost.Object);
            var command = new SendMessageCommand("conv-1", "deleted-user", "Hello");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCodes.AccountDeleted);
        }

        [Fact]
        public async Task GetJobDetails_Should_Return_AccountDeleted_When_User_Is_Deleted()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "deleted-user", FullName = "Deleted User", IsDeleted = true };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var handler = new GetJobDetailsQueryHandler(_context);
            var query = new GetJobDetailsQuery("job-1", "deleted-user");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCodes.AccountDeleted);
        }

        [Fact]
        public async Task ToggleSavedJob_Should_Return_AccountDeleted_When_User_Is_Deleted()
        {
            // Arrange
            var user = new Entities.Users.User { Id = "deleted-user", FullName = "Deleted User", IsDeleted = true };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var handler = new ToggleSavedJobCommandHandler(_context);
            var command = new ToggleSavedJobCommand("job-1", "deleted-user");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCodes.AccountDeleted);
        }
    }
}
