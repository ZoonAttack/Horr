using Entities.Payment;
using Entities.Enums;

namespace Horr.DTOs.Wallet.Transactions
{
    public static class TransactionExtensions
    {
        public static TransactionReadDTO Transaction_To_TransactionRead(this Transaction transaction)
        {
            if (transaction == null)
            {
                return null;
            }

            return new TransactionReadDTO
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                Direction = transaction.Direction,
                Description = transaction.Description,
                RelatedDepositRequestId = transaction.RelatedDepositRequestId,
                CreatedAt = transaction.CreatedAt
            };
        }

        public static Transaction TransactionCreate_To_Transaction(this TransactionCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Transaction
            {
                Amount = createDto.Amount,
                Description = createDto.Description,
                // Direction and UserId would be set by the service
            };
        }

        public static void TransactionStatusUpdate_To_Transaction(this Transaction transaction, TransactionStatusUpdateDTO updateDto)
        {
            // The new Transaction entity doesn't have a Status field directly, 
            // but we might update it via a service. For now, this is a placeholder 
            // to maintain build integrity if the DTO is still used.
        }
    }
}
