using Entities;
using Entities.Enums;
using Entities.Payment;
using Entities.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Implementations.Wallet;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTesting.Wallet
{
    public class HandlerTests : IDisposable
    {
        private readonly AppDbContext _context;

        public HandlerTests()
        {
            _context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task SubmitDeposit_ValidRequest_PersistsPendingWithPhoto()
        {
            // Arrange
            var userId = "user1";
            _context.Users.Add(new Entities.Users.User { Id = userId, UserName = "u1", Email = "u1@t.com", FullName = "U1", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "Egypt", Bio = "B" });
            await _context.SaveChangesAsync();

            var handler = new SubmitDepositRequestCommandHandler(_context);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("receipt.png");
            var command = new SubmitDepositRequestCommand(userId, 100, "REC123", fileMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var persisted = await _context.DepositRequests.FirstOrDefaultAsync(r => r.ReceiptNumber == "REC123");
            Assert.NotNull(persisted);
            Assert.Equal(DepositStatus.Pending, persisted.Status);
            Assert.False(string.IsNullOrEmpty(persisted.ReceiptPhotoUrl));
            Assert.Equal(100, result.Data.Amount);
        }

        [Fact]
        public async Task ReviewDeposit_Approved_UpdatesBalanceAndCreatesTransaction()
        {
            // Arrange
            var userId = "user1";
            _context.Users.Add(new Entities.Users.User { Id = userId, UserName = "u1", Email = "u1@t.com", FullName = "U1", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "Egypt", Bio = "B" });
            var deposit = new DepositRequest { Id = Guid.NewGuid().ToString(), ClientId = userId, Amount = 500, ReceiptNumber = "R1", ReceiptPhotoUrl = "P1", Status = DepositStatus.Pending };
            _context.DepositRequests.Add(deposit);
            _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 100 });
            await _context.SaveChangesAsync();

            var handler = new ReviewDepositRequestCommandHandler(_context);
            var command = new ReviewDepositRequestCommand(deposit.Id, DepositStatus.Approved, "Good");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var updatedDeposit = await _context.DepositRequests.FindAsync(deposit.Id);
            var wallet = await _context.WalletBalances.FirstAsync(w => w.UserId == userId);
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.UserId == userId);

            Assert.Equal(DepositStatus.Approved, updatedDeposit!.Status);
            Assert.Equal("Good", updatedDeposit.AdminNote);
            Assert.Equal(600, wallet.BalanceEGP);
            Assert.NotNull(transaction);
            Assert.Equal(500, transaction.Amount);
            Assert.Equal(TransactionDirection.Credit, transaction.Direction);
        }

        [Fact]
        public async Task ReviewDeposit_Rejected_DoesNotChangeBalance()
        {
            // Arrange
            var userId = "user1";
            _context.Users.Add(new Entities.Users.User { Id = userId, UserName = "u1", Email = "u1@t.com", FullName = "U1", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "Egypt", Bio = "B" });
            var deposit = new DepositRequest { Id = Guid.NewGuid().ToString(), ClientId = userId, Amount = 500, ReceiptNumber = "R1", ReceiptPhotoUrl = "P1", Status = DepositStatus.Pending };
            _context.DepositRequests.Add(deposit);
            _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 100 });
            await _context.SaveChangesAsync();

            var handler = new ReviewDepositRequestCommandHandler(_context);
            var command = new ReviewDepositRequestCommand(deposit.Id, DepositStatus.Rejected, "Fake");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var wallet = await _context.WalletBalances.FirstAsync(w => w.UserId == userId);
            var transactionCount = await _context.Transactions.CountAsync();

            Assert.Equal(100, wallet.BalanceEGP);
            Assert.Equal(0, transactionCount);
        }

        [Fact]
        public async Task SubmitWithdrawal_InsufficientBalance_ReturnsError()
        {
            // Arrange
            var userId = "u1";
            _context.Users.Add(new Entities.Users.User { Id = userId, UserName = "u1", Email = "u1@t.com", FullName = "U1", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "Egypt", Bio = "B" });
            _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 500 });
            await _context.SaveChangesAsync();

            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand(userId, 1000, WithdrawalMethod.InstaPay, "insta", null, null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ServiceImplementation.Helpers.ErrorCodes.InsufficientBalance);
        }

        [Fact]
        public async Task ReviewWithdrawal_Approved_UpdatesStatusButNotBalance()
        {
            // Arrange
            var userId = "u1";
            _context.Users.Add(new Entities.Users.User { Id = userId, UserName = "u1", Email = "u1@t.com", FullName = "U1", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "Egypt", Bio = "B" });
            var withdrawal = new WithdrawalRequest { Id = Guid.NewGuid().ToString(), FreelancerId = userId, Amount = 100, Method = WithdrawalMethod.InstaPay, InstapayUsername = "user_insta", Status = WithdrawalStatus.Pending };
            _context.WithdrawalRequests.Add(withdrawal);
            _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 500 });
            await _context.SaveChangesAsync();

            var handler = new ReviewWithdrawalRequestCommandHandler(_context);
            var command = new ReviewWithdrawalRequestCommand(withdrawal.Id, WithdrawalStatus.Approved, "Sent");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var updated = await _context.WithdrawalRequests.FindAsync(withdrawal.Id);
            var wallet = await _context.WalletBalances.FirstAsync(w => w.UserId == userId);

            Assert.Equal(WithdrawalStatus.Approved, updated!.Status);
            Assert.Equal("Sent", updated.AdminNote);
            Assert.Equal(500, wallet.BalanceEGP); // Should NOT change
        }

        [Fact]
        public async Task ReviewWithdrawal_Rejected_RestoresBalance()
        {
            // Arrange
            var userId = "u1";
            var withdrawal = new WithdrawalRequest { Id = Guid.NewGuid().ToString(), FreelancerId = userId, Amount = 100, Method = WithdrawalMethod.InstaPay, InstapayUsername = "user_insta", Status = WithdrawalStatus.Pending };
            _context.WithdrawalRequests.Add(withdrawal);
            _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 500 });
            await _context.SaveChangesAsync();

            var handler = new ReviewWithdrawalRequestCommandHandler(_context);
            var command = new ReviewWithdrawalRequestCommand(withdrawal.Id, WithdrawalStatus.Rejected, "Invalid");

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updated = await _context.WithdrawalRequests.FindAsync(withdrawal.Id);
            var wallet = await _context.WalletBalances.FirstAsync(w => w.UserId == userId);

            Assert.Equal(WithdrawalStatus.Rejected, updated!.Status);
            Assert.Equal(600, wallet.BalanceEGP); // Should increase from 500 to 600 (refunded)
        }

        [Fact]
        public async Task ReviewWithdrawal_AlreadyReviewed_ThrowsInvalidStateException()
        {
            // Arrange
            var withdrawal = new WithdrawalRequest { Id = Guid.NewGuid().ToString(), FreelancerId = "u1", Amount = 100, Status = WithdrawalStatus.Rejected };
            _context.WithdrawalRequests.Add(withdrawal);
            await _context.SaveChangesAsync();

            var handler = new ReviewWithdrawalRequestCommandHandler(_context);
            var command = new ReviewWithdrawalRequestCommand(withdrawal.Id, WithdrawalStatus.Approved, "Oops");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidStateException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Only pending withdrawal requests can be reviewed.", ex.Message);
        }

        [Fact]
        public async Task GetPendingDepositRequests_ReturnsOnlyPending()
        {
            // Arrange
            _context.DepositRequests.AddRange(new List<DepositRequest>
            {
                new() { Id = Guid.NewGuid().ToString(), ClientId = "u1", Amount = 100, ReceiptNumber = "R1", ReceiptPhotoUrl = "P1", Status = DepositStatus.Pending },
                new() { Id = Guid.NewGuid().ToString(), ClientId = "u2", Amount = 200, ReceiptNumber = "R2", ReceiptPhotoUrl = "P2", Status = DepositStatus.Approved },
                new() { Id = Guid.NewGuid().ToString(), ClientId = "u3", Amount = 300, ReceiptNumber = "R3", ReceiptPhotoUrl = "P3", Status = DepositStatus.Pending }
            });
            await _context.SaveChangesAsync();

            var queryHandler = new WalletQueryHandlers(_context);
            var query = new GetPendingDepositRequestsQuery();

            // Act
            var result = await queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(DepositStatus.Pending, r.Status));
        }

        [Fact]
        public async Task GetWalletBalance_ReturnsCorrectBalance()
        {
            // Arrange
            var userId = "user123";
            _context.Users.Add(new Entities.Users.User { Id = userId, UserName = "u123", Email = "u123@t.com", FullName = "U123", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "Egypt", Bio = "B" });
            _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 1234.56m });
            await _context.SaveChangesAsync();

            var queryHandler = new WalletQueryHandlers(_context);
            var query = new GetWalletBalanceQuery(userId);

            // Act
            var result = await queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(userId, result.Data.UserId);
            Assert.Equal(1234.56m, result.Data.BalanceEGP);
        }
    }
}
