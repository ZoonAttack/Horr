using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;

namespace ServiceImplementation.Mappings
{
    public static class FinancialMappingExtensions
    {
        public static DepositRequestDto ToDto(this DepositRequest depositRequest)
        {
            if (depositRequest == null) return null;

            return new DepositRequestDto
            {
                Id = depositRequest.Id,
                ClientId = depositRequest.ClientId,
                Amount = depositRequest.Amount,
                ReceiptNumber = depositRequest.ReceiptNumber,
                ReceiptPhotoUrl = depositRequest.ReceiptPhotoUrl,
                Status = depositRequest.Status,
                AdminNote = depositRequest.AdminNote,
                SubmittedAt = depositRequest.SubmittedAt,
                ReviewedAt = depositRequest.ReviewedAt
            };
        }

        public static WithdrawalRequestDto ToDto(this WithdrawalRequest withdrawalRequest)
        {
            if (withdrawalRequest == null) return null;

            var dto = new WithdrawalRequestDto
            {
                Id = withdrawalRequest.Id,
                FreelancerId = withdrawalRequest.FreelancerId,
                FreelancerName = withdrawalRequest.Freelancer?.FullName ?? withdrawalRequest.Freelancer?.UserName,
                Amount = withdrawalRequest.Amount,
                Method = withdrawalRequest.Method,
                Status = withdrawalRequest.Status,
                AdminNote = withdrawalRequest.AdminNote,
                SubmittedAt = withdrawalRequest.SubmittedAt,
                ReviewedAt = withdrawalRequest.ReviewedAt,
                InstapayUsername = null,
                BankAccountDetails = null,
                EWalletNumber = null
            };

            switch (withdrawalRequest.Method)
            {
                case WithdrawalMethod.InstaPay:
                    dto.InstapayUsername = withdrawalRequest.InstapayUsername;
                    break;
                case WithdrawalMethod.BankTransfer:
                    dto.BankAccountDetails = withdrawalRequest.BankAccountDetails;
                    break;
                case WithdrawalMethod.EWallet:
                    dto.EWalletNumber = withdrawalRequest.EWalletNumber;
                    break;
            }

            return dto;
        }

        public static WalletBalanceDto ToDto(this WalletBalance walletBalance)
        {
            if (walletBalance == null) return null;

            return new WalletBalanceDto
            {
                Id = walletBalance.Id,
                UserId = walletBalance.UserId,
                BalanceEGP = walletBalance.BalanceEGP,
                LastUpdatedAt = walletBalance.LastUpdatedAt
            };
        }

        public static TransactionDto ToDto(this Transaction transaction)
        {
            if (transaction == null) return null;

            return new TransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                Direction = transaction.Direction,
                Description = transaction.Description,
                RelatedDepositRequestId = transaction.RelatedDepositRequestId,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
