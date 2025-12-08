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
                ApprovedByUserId = request.ApprovedByUserId.HasValue ? request.ApprovedByUserId.Value.ToString() : null,
                ProcessedAt = request.ProcessedAt,
                PaymentMethodId = request.PaymentMethodId.ToString(),
                TransactionId = request.TransactionId.HasValue ? request.TransactionId.Value.ToString() : null,
                RequestedAt = request.RequestedAt
            };
        }

        public static WithdrawalRequest ToWithdrawalRequest(this WithdrawalRequestCreateDTO createDto, string userId)
        {
            if (createDto == null) return null;

            return new WithdrawalRequest
            {
                UserId = userId,
                PaymentMethodId = long.Parse(createDto.PaymentMethodId),
                Amount = createDto.Amount
            };
        }

        public static void ApplyUpdate(this WithdrawalRequest request, RequestStatus status, long adminId, long? transactionId = null)
        {
            request.Status = status.ToString();
            request.ApprovedByUserId = adminId;
            request.ProcessedAt = DateTime.UtcNow;
            if (transactionId.HasValue)
            {
                request.TransactionId = transactionId.Value;
            }
        }
    }
}
