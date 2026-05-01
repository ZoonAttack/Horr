using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet
{
    public class TransactionDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionDirection Direction { get; set; }
        public string Description { get; set; }
        public string? RelatedDepositRequestId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
