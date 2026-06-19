using System;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record RequestAdditionalRevisionCommand(
        Guid DeliveryId,
        string ClientId,
        int RequestedCount,
        string Reason
    ) : IRequest<Result<AdditionalRevisionRequestDto>>;
}