using System;
using Entities.Enums;
using Entities.Payment;

namespace ServiceContracts.DTOs.Wallet.PaymentTransaction
{
    public class PaymentTransactionReadDTO
    {
        public string TransactionId { get; set; }
        public PaymentTransactionRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}