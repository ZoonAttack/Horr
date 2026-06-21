using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class ContractReadDTO
    {
        // Primary identifiers
        public int Id { get; set; }

        public int? ProposalId { get; set; }

        public string ClientId { get; set; }

        public string FreelancerId { get; set; }

        // Optional display names
        public string Proposal_Title { get; set; }

        public string Client_Name { get; set; }

        public string Freelancer_Name { get; set; }

        // Contract details
        public decimal AgreedRate { get; set; }
        public string OriginalCurrency { get; set; } = "USD";
        public decimal? ConvertedAgreedRate { get; set; }
        public string? ConvertedCurrency { get; set; }

        public ContractStatus Status { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int MaxRevisions { get; set; }
        public string? LatestDeliverySummary { get; set; }
        public bool InDispute { get; set; }

        // New details for milestone deliverables and description
        public string? Description { get; set; }
        public List<ContractMilestoneDto>? Milestones { get; set; }
    }
}
