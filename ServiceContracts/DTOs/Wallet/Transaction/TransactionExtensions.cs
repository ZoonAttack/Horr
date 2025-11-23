using Entities.Payment;
using Entities.Enums;

namespace Application.DTOs.Payment
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
                SenderWalletId = transaction.SenderWalletId,
                ReceiverWalletId = transaction.ReceiverWalletId,
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType,
                Status = transaction.Status,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt,
                CompletedAt = transaction.CompletedAt
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
                SenderWalletId = createDto.SenderWalletId,
                ReceiverWalletId = createDto.ReceiverWalletId,
                Amount = createDto.Amount,
                TransactionType = createDto.TransactionType,
                Description = createDto.Description,
            };
        }

        public static void TransactionStatusUpdate_To_Transaction(this Transaction transaction, TransactionStatusUpdateDTO updateDto)
        {
            if (transaction == null || updateDto == null)
            {
                return;
            }

            // Only update the Status and set CompletedAt if the transaction is moving from Pending/Processing to Finalized
            if (transaction.Status != updateDto.Status)
            {
                transaction.Status = updateDto.Status;

                if (updateDto.Status == TransactionStatus.Completed || updateDto.Status == TransactionStatus.Failed)
                {
                    transaction.CompletedAt = DateTime.UtcNow;
                }
            }
        }
    }
}