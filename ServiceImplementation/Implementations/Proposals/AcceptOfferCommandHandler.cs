using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using Entities.Communication;
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

            // State Guard: Assert Proposal.Status = Submitted
            ContractStateGuard.EnsureCanAcceptOffer(proposal);

            // 1. Update Proposal Status to Offer
            proposal.Status = ProposalStatus.Offer;

            // 2. Create Contract from Proposal data
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

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync(cancellationToken);

                // 2b. Automatically create Chat room for the active contract
                var chatExists = await _context.Chats.AnyAsync(c => c.ContractId == contract.Id, cancellationToken);
                if (!chatExists)
                {
                    var chat = new Chat
                    {
                        Id = Guid.NewGuid().ToString(),
                        ContractId = contract.Id,
                        ClientId = contract.ClientId,
                        FreelancerId = contract.FreelancerId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    _context.Chats.Add(chat);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

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
