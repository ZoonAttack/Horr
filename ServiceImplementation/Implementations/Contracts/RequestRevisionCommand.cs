using System;
using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record RequestRevisionCommand(
        Guid DeliveryId,
        string ClientId,
        string Reason
    ) : IRequest<RevisionRequestDto>;
}
