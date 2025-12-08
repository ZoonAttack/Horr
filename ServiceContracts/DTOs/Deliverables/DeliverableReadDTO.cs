using Entities.Enums;

namespace ServiceContracts.DTOs.Deliverables
{
    /// <summary>
    /// DTO for reading or displaying deliverable information.
    /// </summary>
    public class DeliverableReadDTO
    {
        public string Id { get; set; }

        public string ProjectId { get; set; }

        public string MessageId { get; set; }

        public string? ProposalId { get; set; }

        public string FileUrl { get; set; }

        public DeliveryStatus Status { get; set; }

        public DateTime DeliveredAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string ReviewNotes { get; set; }
    }
}
