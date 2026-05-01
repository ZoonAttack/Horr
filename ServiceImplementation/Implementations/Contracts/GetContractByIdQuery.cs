using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetContractByIdQuery(int ContractId, string UserId) : IRequest<ContractReadDTO>;
}
