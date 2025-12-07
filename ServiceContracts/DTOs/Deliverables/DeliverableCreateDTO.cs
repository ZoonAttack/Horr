using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Deliverables
{
    /// <summary>
    /// DTO for creating a new Deliverable.
    /// </summary>
    public class DeliverableCreateDTO
    {
        [Required]
        public long ProjectId { get; set; }

        [Required]
        public long MessageId { get; set; }

        public long? ProposalId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileUrl { get; set; }
    }
}
