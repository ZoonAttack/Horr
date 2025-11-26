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
                Id = request.Id,
                UserId = request.UserId,
                Amount = request.Amount,
                ClientTransactionReference = request.ClientTransactionReference,
                Status = Enum.Parse<RequestStatus>(request.Status),
                ApprovedByUserId = request.ApprovedByUserId,
                ProcessedAt = request.ProcessedAt,
                PaymentMethodId = request.PaymentMethodId,
                TransactionId = request.TransactionId,
                RequestedAt = request.RequestedAt
            };
        }

        public static FundRequest ToFundRequest(this FundRequestCreateDTO createDto, long userId)
        {
            if (createDto == null) return null;

            return new FundRequest
            {
                UserId = userId,
                PaymentMethodId = createDto.PaymentMethodId,
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
