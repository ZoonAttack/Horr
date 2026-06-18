using System;
using MediatR;
using ServiceContracts.DTOs.Contract;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Contracts
{
    public record RequestSpecialistReviewCommand(
        Guid DeliveryId,
        string ClientId,
        ReviewerType ReviewerType,
        string RequirementsSummary
    ) : IRequest<ContractSpecialistReviewReadDto>;
}
