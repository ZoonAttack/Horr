using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractReadDTO>
    {
        private readonly AppDbContext _context;

        public GetContractByIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractReadDTO> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
        {
            var contract = await _context.Contracts
                .Include(c => c.Proposal)
                    .ThenInclude(p => p.JobPost)
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .Include(c => c.WorkDeliveries)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException($"Contract with ID {request.ContractId} not found.");
            }

            // Check if user is part of the contract
            if (contract.ClientId != request.UserId && contract.FreelancerId != request.UserId)
            {
                throw new UnauthorizedAccessException("Unauthorized: You are not a party to this contract.");
            }

            return new ContractReadDTO
            {
                Id = contract.Id,
                ProposalId = contract.ProposalId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                Proposal_Title = contract.Proposal?.JobPost?.Title,
                Client_Name = contract.Client?.FullName,
                Freelancer_Name = contract.Freelancer?.FullName,
                AgreedRate = contract.AgreedRate,
                Status = contract.Status,
                StartedAt = contract.StartedAt,
                ClosedAt = contract.ClosedAt,
                CreatedAt = contract.CreatedAt,
                LatestDeliverySummary = contract.WorkDeliveries
                    .OrderByDescending(d => d.SubmittedAt)
                    .Select(d => d.Note)
                    .FirstOrDefault()
            };
        }
    }
}
