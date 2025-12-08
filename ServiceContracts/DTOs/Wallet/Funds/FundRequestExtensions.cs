using Entities.Enums;
using Entities.Payment;

namespace ServiceContracts.DTOs.Wallet.Funds
{
    public static class FundRequestExtensions
    {
        public static FundRequestReadDTO ToFundRequestRead(this FundRequest request)
        {
            if (request == null) return null;

            return new FundRequestReadDTO
            {
                Id = request.Id.ToString(),
                UserId = request.UserId,
                Amount = request.Amount,
                ClientTransactionReference = request.ClientTransactionReference,
                Status = Enum.Parse<RequestStatus>(request.Status),
                ApprovedByUserId = request.ApprovedByUserId.HasValue ? request.ApprovedByUserId.Value.ToString() : null,
                ProcessedAt = request.ProcessedAt,
                PaymentMethodId = request.PaymentMethodId.ToString(),
                TransactionId = request.TransactionId.HasValue ? request.TransactionId.Value.ToString() : null,
                RequestedAt = request.RequestedAt
            };
        }

        public static FundRequest ToFundRequest(this FundRequestCreateDTO createDto, string userId)
        {
            if (createDto == null) return null;

            return new FundRequest
            {
                UserId = userId,
                PaymentMethodId = long.Parse(createDto.PaymentMethodId),
                Amount = createDto.Amount,
                ClientTransactionReference = createDto.ClientTransactionReference
            };
        }

        public static void ApplyUpdate(this FundRequest request, RequestStatus status, long adminId, long? transactionId = null)
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
