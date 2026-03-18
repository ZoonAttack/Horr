using Entities.Project;

namespace ServiceContracts.DTOs.Contract
{
    public static class WorkDeliveryExtensions
    {
        public static WorkDeliveryDto ToDto(this WorkDelivery delivery)
        {
            if (delivery == null) return null!;

            return new WorkDeliveryDto
            {
                Id = delivery.Id,
                ContractId = delivery.ContractId,
                Note = delivery.Note,
                ActionStatus = delivery.ActionStatus,
                SubmittedAt = delivery.SubmittedAt
            };
        }
    }
}
