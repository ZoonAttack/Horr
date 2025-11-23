using System;
using Entities.Payment;

namespace ServiceContracts.DTOs.Wallet
{
    public class WalletReadDTO
    {
        public long Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public static class WalletExtensions
    {
        public static WalletReadDTO Wallet_To_WalletRead(this Entities.Payment.Wallet wallet)
        {
            if (wallet == null)
            {
                return null;
            }

            return new WalletReadDTO
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt
            };
        }
    }
}