using MediatR;

namespace ServiceImplementation.Implementations.Contracts
{
    public record MarkContractAsCompletedCommand(int ContractId, string ClientId) : IRequest<bool>;
}
