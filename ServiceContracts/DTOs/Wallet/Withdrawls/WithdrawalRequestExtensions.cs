using Entities.Enums;
using Entities.Payment;

namespace ServiceContracts.DTOs.Wallet.Withdrawls
{
    public static class WithdrawalRequestExtensions
    {
        public static WithdrawalRequestReadDTO ToWithdrawalRequestRead(this WithdrawalRequest request)
        {
            if (request == null) return null;

            return new WithdrawalRequestReadDTO
            {
                Id = request.Id,
                FreelancerId = request.FreelancerId,
                Amount = request.Amount,
                Status = request.Status,
                Method = request.Method,
                InstapayUsername = request.InstapayUsername,
                BankAccountDetails = request.BankAccountDetails,
                EWalletNumber = request.EWalletNumber,
                AdminNote = request.AdminNote,
                SubmittedAt = request.SubmittedAt,
                ReviewedAt = request.ReviewedAt
            };
        }

        public static WithdrawalRequest ToWithdrawalRequest(this WithdrawalRequestCreateDTO createDto, string userId)
        {
            if (createDto == null) return null;

            return new WithdrawalRequest
            {
                FreelancerId = userId,
                Amount = createDto.Amount,
                // Additional fields would be mapped from DTO in a full implementation
            };
        }

        public static void ApplyUpdate(this WithdrawalRequest request, WithdrawalStatus status, string? adminNote = null)
        {
            request.Status = status;
            request.AdminNote = adminNote;
            request.ReviewedAt = DateTime.UtcNow;
        }
    }
}
