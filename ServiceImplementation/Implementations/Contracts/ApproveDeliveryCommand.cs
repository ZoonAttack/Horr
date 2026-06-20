using System;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record ApproveDeliveryCommand(
        Guid DeliveryId,
        string ClientId
    ) : IRequest<Result<ContractDeliveryDto>>;
}
