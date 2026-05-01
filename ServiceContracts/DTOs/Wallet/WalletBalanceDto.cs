using System;

namespace ServiceContracts.DTOs.Wallet
{
    public class WalletBalanceDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal BalanceEGP { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
