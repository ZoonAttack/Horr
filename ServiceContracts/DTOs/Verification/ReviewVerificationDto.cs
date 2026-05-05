using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Verification
{
    public class ReviewVerificationDto
    {
        [Required]
        public string RequestId { get; set; }
        
        [Required]
        public bool Approved { get; set; }
        
        [MaxLength(500)]
        public string? RejectionReason { get; set; }  // Required if Approved = false
    }
}
