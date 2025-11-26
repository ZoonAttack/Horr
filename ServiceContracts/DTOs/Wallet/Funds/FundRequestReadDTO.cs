using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet.Funds
{
    public class FundRequestReadDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public string ClientTransactionReference { get; set; }
        public RequestStatus Status { get; set; }
        public long? ApprovedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public long? PaymentMethodId { get; set; }
        public long? TransactionId { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
