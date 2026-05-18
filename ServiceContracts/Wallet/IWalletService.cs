using System.Threading.Tasks;
using Entities.Enums;

namespace Services.Wallet
{
    public interface IWalletService
    {
        Task CreditWalletAsync(string userId, decimal amount, TransactionType type, string description);
        Task DebitWalletAsync(string userId, decimal amount, TransactionType type, string description);
    }
}
