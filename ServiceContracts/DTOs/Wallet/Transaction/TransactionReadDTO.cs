using System;
using Entities.Enums;

namespace Horr.DTOs.Wallet.Transactions
{
    public class TransactionReadDTO
    {
        public string Id { get; set; }
        public string? SenderWalletId { get; set; }
        public string? ReceiverWalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}