using Entities.Enums;

namespace ServiceContracts.DTOs.Proposal
{
    public class ProposalReadDTO
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }
        public string JobPostTitle { get; set; } = string.Empty;
        public string FreelancerId { get; set; } = string.Empty;
        public string FreelancerName { get; set; } = string.Empty;
        public SubmitAsType SubmitAsType { get; set; }
        public decimal BidRate { get; set; }
        public decimal HORRFee { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public ProposalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProposalTermReadDTO> Terms { get; set; } = new List<ProposalTermReadDTO>();
    }

    public class ProposalTermReadDTO
    {
        public int Id { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class MyProposalsResponseDto
    {
        public List<ProposalReadDTO> Active { get; set; } = new List<ProposalReadDTO>();
        public List<ProposalReadDTO> Submitted { get; set; } = new List<ProposalReadDTO>();
        public List<ProposalReadDTO> Offers { get; set; } = new List<ProposalReadDTO>();
    }
}
