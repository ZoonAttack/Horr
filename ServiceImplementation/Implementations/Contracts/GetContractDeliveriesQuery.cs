using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetContractDeliveriesQuery(int ContractId) : IRequest<List<ContractDeliveryDto>>;
}
