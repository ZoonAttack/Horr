using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record RejectContractCommand(int ContractId, string ClientId) : IRequest<Result<bool>>;
}
