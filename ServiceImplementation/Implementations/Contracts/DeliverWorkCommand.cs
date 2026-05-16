using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Contracts
{
    public record DeliverWorkCommand(int ContractId, string Note, string FreelancerId, List<IFormFile>? Files = null) : IRequest<Result<WorkDeliveryDto>>;
}
