using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet
{
    public class WithdrawalRequestDto
    {
        public string Id { get; set; }
        public string FreelancerId { get; set; }
        public decimal Amount { get; set; }
        public WithdrawalMethod Method { get; set; }
        public string? InstapayUsername { get; set; }
        public string? BankAccountDetails { get; set; }
        public string? EWalletNumber { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string? AdminNote { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
