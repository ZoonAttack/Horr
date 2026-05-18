using System;
using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record SubmitDeliveryCommand(
        int ContractId,
        Guid? ContractMilestoneId,
        string? DeliveryNote,
        string FreelancerId,
        List<AttachmentDto> Attachments
    ) : IRequest<ContractDeliveryDto>;
}
