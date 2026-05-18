using System;
using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record OpenDisputeCommand(
        int ContractId,
        Guid DeliveryId,
        string OpenedByUserId,
        string Reason
    ) : IRequest<DisputeDto>;
}
