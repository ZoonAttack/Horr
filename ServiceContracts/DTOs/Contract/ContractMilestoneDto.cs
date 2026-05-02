using System;

namespace ServiceContracts.DTOs.Contract
{
    public class ContractMilestoneDto
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
    }
}
