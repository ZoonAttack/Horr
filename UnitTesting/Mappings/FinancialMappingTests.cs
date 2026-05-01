using Entities.Payment;
using Entities.Enums;
using ServiceImplementation.Mappings;
using Xunit;

namespace UnitTesting.Mappings
{
    public class FinancialMappingTests
    {
        [Fact]
        public void DepositRequest_ToDto_Pending_NullAdminNote_MapsCorrectly()
        {
            // Arrange
            var entity = new DepositRequest
            {
                Id = "DR1",
                Status = DepositStatus.Pending,
                AdminNote = null,
                Amount = 100,
                ClientId = "U1",
                ReceiptNumber = "REC1",
                ReceiptPhotoUrl = "URL1",
                SubmittedAt = DateTime.UtcNow
            };

            // Act
            var dto = entity.ToDto();

            // Assert
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(DepositStatus.Pending, dto.Status);
            Assert.Null(dto.AdminNote);
        }

        [Fact]
        public void DepositRequest_ToDto_Rejected_WithAdminNote_MapsCorrectly()
        {
            // Arrange
            var entity = new DepositRequest
            {
                Id = "DR2",
                Status = DepositStatus.Rejected,
                AdminNote = "Unreadable",
                Amount = 200,
                ClientId = "U2",
                ReceiptNumber = "REC2",
                ReceiptPhotoUrl = "URL2",
                SubmittedAt = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow
            };

            // Act
            var dto = entity.ToDto();

            // Assert
            Assert.Equal(DepositStatus.Rejected, dto.Status);
            Assert.Equal("Unreadable", dto.AdminNote);
        }

        [Fact]
        public void WithdrawalRequest_ToDto_InstaPay_PopulatesOnlyInstapayField()
        {
            // Arrange
            var entity = new WithdrawalRequest
            {
                Id = "WR1",
                Method = WithdrawalMethod.InstaPay,
                InstapayUsername = "user_insta",
                BankAccountDetails = "some bank",
                EWalletNumber = "0123456",
                FreelancerId = "F1",
                Amount = 500,
                Status = WithdrawalStatus.Pending
            };

            // Act
            var dto = entity.ToDto();

            // Assert
            Assert.Equal(WithdrawalMethod.InstaPay, dto.Method);
            Assert.Equal("user_insta", dto.InstapayUsername);
            Assert.Null(dto.BankAccountDetails);
            Assert.Null(dto.EWalletNumber);
        }

        [Fact]
        public void WithdrawalRequest_ToDto_BankTransfer_PopulatesOnlyBankField()
        {
            // Arrange
            var entity = new WithdrawalRequest
            {
                Id = "WR2",
                Method = WithdrawalMethod.BankTransfer,
                InstapayUsername = "user_insta",
                BankAccountDetails = "bank account info",
                EWalletNumber = "0123456",
                FreelancerId = "F2",
                Amount = 600,
                Status = WithdrawalStatus.Pending
            };

            // Act
            var dto = entity.ToDto();

            // Assert
            Assert.Equal(WithdrawalMethod.BankTransfer, dto.Method);
            Assert.Equal("bank account info", dto.BankAccountDetails);
            Assert.Null(dto.InstapayUsername);
            Assert.Null(dto.EWalletNumber);
        }

        [Fact]
        public void WithdrawalRequest_ToDto_EWallet_PopulatesOnlyEWalletField()
        {
            // Arrange
            var entity = new WithdrawalRequest
            {
                Id = "WR3",
                Method = WithdrawalMethod.EWallet,
                InstapayUsername = "user_insta",
                BankAccountDetails = "some bank",
                EWalletNumber = "0100200300",
                FreelancerId = "F3",
                Amount = 700,
                Status = WithdrawalStatus.Pending
            };

            // Act
            var dto = entity.ToDto();

            // Assert
            Assert.Equal(WithdrawalMethod.EWallet, dto.Method);
            Assert.Equal("0100200300", dto.EWalletNumber);
            Assert.Null(dto.InstapayUsername);
            Assert.Null(dto.BankAccountDetails);
        }
    }
}
