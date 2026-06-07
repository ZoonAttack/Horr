using Entities.Project;
using System.Linq;

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
                RevisionNote = delivery.RevisionNote,
                ActionStatus = delivery.ActionStatus,
                SubmittedAt = delivery.SubmittedAt,
                Attachments = delivery.Attachments?.Select(a => a.ToDto()).ToList() ?? new()
            };
        }
    }
}
