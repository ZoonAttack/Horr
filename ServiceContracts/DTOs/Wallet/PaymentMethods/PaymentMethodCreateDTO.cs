using Entities.Enums;


namespace ServiceContracts.DTOs.Wallet.PaymentMethods
{
    public class PaymentMethodCreateDTO
    {
        public PaymentMethodTypes MethodName { get; set; }
        public string AccountIdentifier { get; set; }
    }
}
