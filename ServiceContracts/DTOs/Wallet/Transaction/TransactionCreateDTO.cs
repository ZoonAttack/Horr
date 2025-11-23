using Entities.Enums;

namespace Application.DTOs.Payment
{
    public class TransactionCreateDTO
    {
        public long? SenderWalletId { get; set; }
        public long? ReceiverWalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
    }
}