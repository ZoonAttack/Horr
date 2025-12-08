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
                Id = delivery.Id.ToString(),
                ProjectId = delivery.ProjectId.ToString(),
                MessageId = delivery.MessageId.ToString(),
                ProposalId = delivery.ProposalId.HasValue ? delivery.ProposalId.Value.ToString() : null,
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
                ProjectId = long.Parse(createDto.ProjectId),
                MessageId = long.Parse(createDto.MessageId),
                ProposalId = string.IsNullOrWhiteSpace(createDto.ProposalId) ? null : long.Parse(createDto.ProposalId),
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

            // Set ReviewedAt when delivery is reviewed (approved or rejected)
            if ((updateDto.Status == DeliveryStatus.Approved || updateDto.Status == DeliveryStatus.Rejected)
                && delivery.ReviewedAt == null)
            {
                delivery.ReviewedAt = DateTime.UtcNow;
            }
        }
    }
}
