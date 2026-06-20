using Entities.Enums;
using System;

namespace ServiceContracts.DTOs.Proposal
{
    public class ClientProposalSummaryDto
    {
        public int Id { get; set; }
        public string FreelancerId { get; set; } = string.Empty;
        public string FreelancerName { get; set; } = string.Empty;
        public decimal BidRate { get; set; }
        public string BidCurrency { get; set; } = "USD";
        public string CoverLetter { get; set; } = string.Empty;
        public ProposalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Job summary details
        public string JobPostId { get; set; } = string.Empty;
        public string JobPostTitle { get; set; } = string.Empty;
        public decimal JobBudget { get; set; }
        public string JobBudgetCurrency { get; set; } = "USD";
        public JobType JobType { get; set; }
    }
}
