using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Payment;
using Entities.Enums;
using Services.Wallet;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Wallet
{
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;

        public WalletService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreditWalletAsync(string userId, decimal amount, TransactionType type, string description)
        {
            if (amount <= 0)
            {
                throw new ValidationException("Credit amount must be greater than zero.");
            }

            var wallet = await _context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new WalletBalance
                {
                    UserId = userId,
                    BalanceEGP = 0,
                    LastUpdatedAt = DateTime.UtcNow
                };
                _context.WalletBalances.Add(wallet);
            }

            wallet.BalanceEGP += amount;
            wallet.LastUpdatedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Direction = TransactionDirection.Credit,
                Type = type,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
        }

        public async Task DebitWalletAsync(string userId, decimal amount, TransactionType type, string description)
        {
            if (amount <= 0)
            {
                throw new ValidationException("Debit amount must be greater than zero.");
            }

            var wallet = await _context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null || wallet.BalanceEGP < amount)
            {
                throw new ValidationException("Insufficient wallet balance.");
            }

            wallet.BalanceEGP -= amount;
            wallet.LastUpdatedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Direction = TransactionDirection.Debit,
                Type = type,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
        }
    }
}
