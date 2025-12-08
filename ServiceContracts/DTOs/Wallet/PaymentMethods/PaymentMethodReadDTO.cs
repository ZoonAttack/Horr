

namespace ServiceContracts.DTOs.Wallet.PaymentMethods
{
    public class PaymentMethodReadDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string MethodName { get; set; }
        public string AccountIdentifier { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
