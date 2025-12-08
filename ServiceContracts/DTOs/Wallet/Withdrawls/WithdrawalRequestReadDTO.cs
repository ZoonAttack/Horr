using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet.Withdrawls
{
    public class WithdrawalRequestReadDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public RequestStatus Status { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? PaymentMethodId { get; set; }
        public string? TransactionId { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
