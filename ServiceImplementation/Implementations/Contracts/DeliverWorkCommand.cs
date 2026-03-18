using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record DeliverWorkCommand(int ContractId, string Note, string FreelancerId) : IRequest<WorkDeliveryDto>;
}
