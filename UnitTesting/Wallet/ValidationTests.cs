using Entities;
using Entities.Enums;
using Entities.Payment;
using Microsoft.AspNetCore.Http;
using Moq;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Implementations.Wallet;
using Xunit;

namespace UnitTesting.Wallet
{
    public class ValidationTests : IDisposable
    {
        private readonly AppDbContext _context;

        public ValidationTests()
        {
            _context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task SubmitDeposit_AmountLessThanOrEqualToZero_ThrowsValidationException(decimal amount)
        {
            // Arrange
            var handler = new SubmitDepositRequestCommandHandler(_context);
            var fileMock = new Mock<IFormFile>();
            var command = new SubmitDepositRequestCommand("user1", amount, "REC123", fileMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Amount must be greater than zero.", ex.Errors);
        }

        [Fact]
        public async Task SubmitDeposit_MissingReceiptNumber_ThrowsValidationException()
        {
            // Arrange
            var handler = new SubmitDepositRequestCommandHandler(_context);
            var fileMock = new Mock<IFormFile>();
            var command = new SubmitDepositRequestCommand("user1", 100, "", fileMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Receipt number is required.", ex.Errors);
        }

        [Fact]
        public async Task SubmitDeposit_MissingReceiptPhoto_ThrowsValidationException()
        {
            // Arrange
            var handler = new SubmitDepositRequestCommandHandler(_context);
            var command = new SubmitDepositRequestCommand("user1", 100, "REC123", null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Receipt photo is required.", ex.Errors);
        }

        [Fact]
        public async Task SubmitWithdrawal_InstaPay_EmptyUsername_ThrowsValidationException()
        {
            // Arrange
            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand("user1", 100, WithdrawalMethod.InstaPay, "", null, null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("InstaPay username is required.", ex.Errors);
        }

        [Fact]
        public async Task SubmitWithdrawal_BankTransfer_EmptyDetails_ThrowsValidationException()
        {
            // Arrange
            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand("user1", 100, WithdrawalMethod.BankTransfer, null, "", null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Bank account details are required.", ex.Errors);
        }

        [Fact]
        public async Task SubmitWithdrawal_EWallet_EmptyNumber_ThrowsValidationException()
        {
            // Arrange
            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand("user1", 100, WithdrawalMethod.EWallet, null, null, "");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("E-wallet number is required.", ex.Errors);
        }

        [Fact]
        public async Task SubmitWithdrawal_InsufficientBalance_ThrowsValidationException()
        {
            // Arrange
            var userId = "user1";
            _context.WalletBalances.Add(new WalletBalance { UserId = userId, BalanceEGP = 500 });
            await _context.SaveChangesAsync();

            var handler = new SubmitWithdrawalRequestCommandHandler(_context);
            var command = new SubmitWithdrawalRequestCommand(userId, 1000, WithdrawalMethod.InstaPay, "user_insta", null, null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Insufficient wallet balance.", ex.Errors);
        }
    }
}
