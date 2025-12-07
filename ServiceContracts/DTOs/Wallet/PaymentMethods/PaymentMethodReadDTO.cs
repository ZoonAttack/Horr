

namespace ServiceContracts.DTOs.Wallet.PaymentMethods
{
    public class PaymentMethodReadDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string MethodName { get; set; }
        public string AccountIdentifier { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
