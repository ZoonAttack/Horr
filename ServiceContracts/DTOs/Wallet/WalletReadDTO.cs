using System;
using Entities.Payment;

namespace ServiceContracts.DTOs.Wallet
{
    public class WalletReadDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
