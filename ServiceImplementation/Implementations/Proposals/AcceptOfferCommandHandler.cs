using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Proposals
{
    public class AcceptOfferCommandHandler : IRequestHandler<AcceptOfferCommand, ContractReadDTO>
    {
        private readonly AppDbContext _context;

        public AcceptOfferCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractReadDTO> Handle(AcceptOfferCommand request, CancellationToken cancellationToken)
        {
            var proposal = await _context.Proposals
                .Include(p => p.JobPost)
                .Include(p => p.Freelancer)
                .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

            if (proposal == null)
            {
                throw new NotFoundException($"Proposal with ID {request.ProposalId} not found.");
            }

            if (proposal.FreelancerId != request.FreelancerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: You can only accept offers sent to you.");
            }

            // State Guard
            ContractStateGuard.EnsureCanAcceptOffer(proposal);

            // 1. Update Proposal Status to Active (or accepted)
            proposal.Status = ProposalStatus.Active;

            // 2. Create Contract
            var contract = new Contract
            {
                ProposalId = proposal.Id,
                ClientId = proposal.JobPost.ClientId,
                FreelancerId = proposal.FreelancerId,
                AgreedRate = proposal.BidRate,
                Status = ContractStatus.Active,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync(cancellationToken);

            // 3. Return Read DTO
            return new ContractReadDTO
            {
                Id = contract.Id,
                ProposalId = contract.ProposalId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                AgreedRate = contract.AgreedRate,
                Status = contract.Status,
                StartedAt = contract.StartedAt,
                CreatedAt = contract.CreatedAt
            };
        }
    }
}
