using System;
using Entities.Enums;

namespace Application.DTOs.Payment
{
    public class TransactionReadDTO
    {
        public long Id { get; set; }
        public long? SenderWalletId { get; set; }
        public long? ReceiverWalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}