using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet.PaymentTransaction
{
    public class PaymentTransactionCreateDTO
    {
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public PaymentTransactionRole Role { get; set; }
    }
}