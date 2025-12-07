using Entities.Enums;

namespace ServiceContracts.DTOs.Deliverables
{
    /// <summary>
    /// DTO for reading or displaying deliverable information.
    /// </summary>
    public class DeliverableReadDTO
    {
        public long Id { get; set; }

        public long ProjectId { get; set; }

        public long MessageId { get; set; }

        public long? ProposalId { get; set; }

        public string FileUrl { get; set; }

        public DeliveryStatus Status { get; set; }

        public DateTime DeliveredAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string ReviewNotes { get; set; }
    }
}
