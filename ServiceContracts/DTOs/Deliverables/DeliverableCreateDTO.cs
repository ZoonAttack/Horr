using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Deliverables
{
    /// <summary>
    /// DTO for creating a new Deliverable.
    /// </summary>
    public class DeliverableCreateDTO
    {
        [Required]
        public string ProjectId { get; set; }

        [Required]
        public string MessageId { get; set; }

        public string? ProposalId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileUrl { get; set; }
    }
}
