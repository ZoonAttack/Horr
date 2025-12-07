using Entities.Enums;

namespace ServiceContracts.DTOs.Deliverables
{
    /// <summary>
    /// DTO for updating deliverable status and review notes.
    /// </summary>
    public class DeliverableUpdateDTO
    {
        public DeliveryStatus Status { get; set; }

        public string ReviewNotes { get; set; }
    }
}
