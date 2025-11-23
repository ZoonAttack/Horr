using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet.PaymentTransaction
{
    public class PaymentTransactionCreateDTO
    {
        public long PaymentId { get; set; }
        public long TransactionId { get; set; }
        public PaymentTransactionRole Role { get; set; }
    }
}