using System;

namespace ServiceContracts.DTOs.Contract
{
    public class ContractMilestoneDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string OriginalCurrency { get; set; } = "USD";
        public decimal? ConvertedAmount { get; set; }
        public string? ConvertedCurrency { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
