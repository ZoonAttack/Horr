using Entities.Enums;
using Entities.Project;

namespace ServiceContracts.DTOs.Deliverables
{
    public static class DeliverableExtensions
    {
        /// <summary>
        /// Converts Delivery entity to DeliverableReadDTO
        /// </summary>
        public static DeliverableReadDTO Delivery_To_DeliverableRead(this Delivery delivery)
        {
            if (delivery == null)
            {
                return null;
            }

            return new DeliverableReadDTO
            {
                Id = delivery.Id,
                ProjectId = delivery.ProjectId,
                MessageId = delivery.MessageId,
                ProposalId = delivery.ProposalId,
                FileUrl = delivery.FileUrl,
                Status = delivery.Status,
                DeliveredAt = delivery.DeliveredAt,
                ReviewedAt = delivery.ReviewedAt,
                ReviewNotes = delivery.ReviewNotes
            };
        }

        /// <summary>
        /// Converts DeliverableCreateDTO to Delivery entity
        /// </summary>
        public static Delivery DeliverableCreate_To_Delivery(this DeliverableCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Delivery
            {
                ProjectId = createDto.ProjectId,
                MessageId = createDto.MessageId,
                ProposalId = createDto.ProposalId,
                FileUrl = createDto.FileUrl,
                Status = DeliveryStatus.Pending
            };
        }

        /// <summary>
        /// Applies DeliverableUpdateDTO to an existing Delivery entity
        /// </summary>
        public static void DeliverableUpdate_To_Delivery(this Delivery delivery, DeliverableUpdateDTO updateDto)
        {
            if (delivery == null || updateDto == null)
            {
                return;
            }

            delivery.Status = updateDto.Status;

            if (!string.IsNullOrEmpty(updateDto.ReviewNotes))
                delivery.ReviewNotes = updateDto.ReviewNotes;

            // Set ReviewedAt when delivery is reviewed
            if ((updateDto.Status == DeliveryStatus.Accepted || updateDto.Status == DeliveryStatus.Rejected) 
                && delivery.ReviewedAt == null)
            {
                delivery.ReviewedAt = DateTime.UtcNow;
            }
        }
    }
}
