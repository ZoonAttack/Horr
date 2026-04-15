using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTOs.Contract;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Contracts
{
    public record DeliverWorkCommand(int ContractId, string Note, string FreelancerId, List<IFormFile>? Files = null) : IRequest<WorkDeliveryDto>;
}
