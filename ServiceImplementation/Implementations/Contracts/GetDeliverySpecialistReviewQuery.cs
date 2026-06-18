using System;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetDeliverySpecialistReviewQuery(
        Guid DeliveryId,
        string RequestingUserId
    ) : IRequest<Result<ContractSpecialistReviewReadDto>>;
}
