using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetContractByIdQuery(int ContractId, string UserId) : IRequest<Result<ContractReadDTO>>;
}
