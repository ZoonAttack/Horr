using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Proposal
{
    public class ProposalUpdateDTO
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Bid rate must be greater than 0")]
        public decimal BidRate { get; set; }

        [Required]
        [MinLength(50, ErrorMessage = "Cover letter must be at least 50 characters")]
        [MaxLength(2000, ErrorMessage = "Cover letter cannot exceed 2000 characters")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9\s\.,!?]+$", ErrorMessage = "Cover letter contains invalid characters")]
        public string CoverLetter { get; set; } = string.Empty;
    }
}
