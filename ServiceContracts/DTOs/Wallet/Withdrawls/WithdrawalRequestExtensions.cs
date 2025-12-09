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
                Id = request.Id.ToString(),
                UserId = request.UserId,
                Amount = request.Amount,
                Status = Enum.Parse<RequestStatus>(request.Status),
                ApprovedByUserId = request.ApprovedByUserId,
                ProcessedAt = request.ProcessedAt,
                PaymentMethodId = request.PaymentMethodId.ToString(),
                TransactionId = request.TransactionId,
                RequestedAt = request.RequestedAt
            };
        }

        public static WithdrawalRequest ToWithdrawalRequest(this WithdrawalRequestCreateDTO createDto, string userId)
        {
            if (createDto == null) return null;

            return new WithdrawalRequest
            {
                UserId = userId,
                PaymentMethodId = createDto.PaymentMethodId,
                Amount = createDto.Amount
            };
        }

        public static void ApplyUpdate(this WithdrawalRequest request, RequestStatus status, string adminId, string? transactionId = null)
        {
            request.Status = status.ToString();
            request.ApprovedByUserId = adminId;
            request.ProcessedAt = DateTime.UtcNow;
            if (!String.IsNullOrEmpty(transactionId)) request.TransactionId = transactionId;
        }
    }
}
