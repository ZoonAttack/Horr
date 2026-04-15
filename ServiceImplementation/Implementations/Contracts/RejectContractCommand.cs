using MediatR;

namespace ServiceImplementation.Implementations.Contracts
{
    public record RejectContractCommand(int ContractId, string ClientId) : IRequest<bool>;
}
