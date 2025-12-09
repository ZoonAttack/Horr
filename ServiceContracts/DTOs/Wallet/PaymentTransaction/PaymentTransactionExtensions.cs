namespace ServiceContracts.DTOs.Wallet.PaymentTransaction
{
    public static class PaymentTransactionExtensions
    {
        public static PaymentTransactionReadDTO ToPaymentTransactionRead(this Entities.Payment.PaymentTransaction pt)
        {
            if (pt == null)
            {
                return null;
            }

            return new PaymentTransactionReadDTO
            {
                TransactionId = pt.TransactionId.ToString(),
                Role = pt.Role,
                CreatedAt = pt.CreatedAt
            };
        }

        public static Entities.Payment.PaymentTransaction ToPaymentTransaction(this PaymentTransactionCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Payment.PaymentTransaction
            {
                PaymentId = createDto.PaymentId,
                TransactionId = createDto.TransactionId,
                Role = createDto.Role
            };
        }
    }
}