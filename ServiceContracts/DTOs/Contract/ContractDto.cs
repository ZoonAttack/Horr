using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class ContractDto
    {
        public int Id { get; set; }
        public int? ProposalId { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string FreelancerId { get; set; } = string.Empty;
        public decimal AgreedRate { get; set; }
        public ContractStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int MaxRevisions { get; set; }
    }
}
