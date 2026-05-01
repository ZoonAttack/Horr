using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet
{
    public class DepositRequestDto
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public decimal Amount { get; set; }
        public string ReceiptNumber { get; set; }
        public string ReceiptPhotoUrl { get; set; }
        public DepositStatus Status { get; set; }
        public string? AdminNote { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
