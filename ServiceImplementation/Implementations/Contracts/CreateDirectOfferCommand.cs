using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using Services;
using ServiceContracts.DTOs.Contract;
using Services;

namespace ServiceImplementation.Implementations.Contracts
{
    public class CreateDirectOfferCommand : IRequest<Result<ContractDto>>
    {
        public string ClientId { get; set; } = string.Empty;
        public string FreelancerId { get; set; } = string.Empty;
        public string JobPostId { get; set; } = string.Empty;
        public string CustomJobDescription { get; set; } = string.Empty;
        public List<ContractMilestoneDto> Milestones { get; set; } = new List<ContractMilestoneDto>();
    }
}
