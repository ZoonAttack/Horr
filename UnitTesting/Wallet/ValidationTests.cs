using Entities;
using Entities.Enums;
using Entities.Payment;
using Entities.Users;
using Microsoft.AspNetCore.Http;
using Moq;
using ServiceImplementation.Implementations.Wallet;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using FluentAssertions;

namespace UnitTesting.Wallet
{
    public class ValidationTests : IDisposable
    {
        private readonly AppDbContext _context;

        public ValidationTests()
        {
            _context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            // Seed a valid user to pass the initial account check
            _context.Users.Add(new Entities.Users.User { Id = "user1", FullName = "Test User", UserName = "user1", Email = "user1@test.com" });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task SubmitDeposit_AmountLessThanOrEqualToZero_ReturnsError(decimal amount)
        {
            // Arrange
            var handler = new SubmitDepositRequestCommandHandler(_context);
            var fileMock = new Mock<IFormFile>();
            var command = new SubmitDepositRequestCommand("user1", amount, "REC123", fileMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("Amount must be greater than zero.");
        }

        [Fact]
        public async Task SubmitDeposit_MissingReceiptNumber_ReturnsError()
        {
            // Arrange
            var handler = new SubmitDepositRequestCommandHandler(_context);
            var fileMock = new Mock<IFormFile>();
            var command = new SubmitDepositRequestCommand("user1", 100, "", fileMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("Receipt number is required.");
        }

        [Fact]
        public async Task SubmitDeposit_MissingReceiptPhoto_ReturnsError()
        {
            // Arrange
            var handler = new SubmitDepositRequestCommandHandler(_context);
            var command = new SubmitDepositRequestCommand("user1", 100, "REC123", null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("Receipt photo is required.");
        }

        [Fact]
        public async Task SubmitWithdrawal_InstaPay_EmptyUsername_ReturnsError()
        {
            // Arrange
            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand("user1", 100, WithdrawalMethod.InstaPay, "", null, null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("InstaPay username is required.");
        }

        [Fact]
        public async Task SubmitWithdrawal_BankTransfer_EmptyDetails_ReturnsError()
        {
            // Arrange
            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand("user1", 100, WithdrawalMethod.BankTransfer, null, "", null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("Bank account details are required.");
        }

        [Fact]
        public async Task SubmitWithdrawal_EWallet_EmptyNumber_ReturnsError()
        {
            // Arrange
            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand("user1", 100, WithdrawalMethod.EWallet, null, null, "");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("E-wallet number is required.");
        }

        [Fact]
        public async Task SubmitWithdrawal_InsufficientBalance_ReturnsError()
        {
            // Arrange
            var userId = "user1";
            var balance = _context.WalletBalances.Local.FirstOrDefault(b => b.UserId == userId);
            if (balance == null)
            {
                _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 500 });
            }
            else
            {
                balance.BalanceEGP = 500;
            }
            await _context.SaveChangesAsync();

            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand(userId, 1000, WithdrawalMethod.InstaPay, "user_insta", null, null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("Insufficient wallet balance.");
        }
    }
}
