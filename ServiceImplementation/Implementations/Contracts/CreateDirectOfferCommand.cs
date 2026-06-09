using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using Services;

namespace ServiceImplementation.Implementations.Contracts
{
    public class CreateDirectOfferCommand : IRequest<Result<ContractDto>>
    {
        public string ClientId { get; set; } = string.Empty;
        public string FreelancerId { get; set; } = string.Empty;
        public string JobPostId { get; set; } = string.Empty;
        public int? ProposalId { get; set; }
        public decimal? AgreedRate { get; set; }
        public string? CustomJobDescription { get; set; }
    }
}
