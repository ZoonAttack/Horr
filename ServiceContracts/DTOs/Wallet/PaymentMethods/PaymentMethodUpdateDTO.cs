

using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet.PaymentMethods
{
    public class PaymentMethodUpdateDTO
    {
        public PaymentMethodTypes Method { get; set; }
        public string AccountIdentifier { get; set; }
    }
}
