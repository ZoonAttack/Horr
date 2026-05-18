using System;
using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record ApproveDeliveryCommand(
        Guid DeliveryId,
        string ClientId
    ) : IRequest<ContractDeliveryDto>;
}
