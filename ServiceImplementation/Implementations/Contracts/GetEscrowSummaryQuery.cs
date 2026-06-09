using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetEscrowSummaryQuery(int ContractId) : IRequest<EscrowSummaryDto>;
}
