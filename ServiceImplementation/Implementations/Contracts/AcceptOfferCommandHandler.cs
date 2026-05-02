using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class AcceptOfferCommandHandler : IRequestHandler<AcceptOfferCommand, bool>
    {
        private readonly AppDbContext _context;

        public AcceptOfferCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AcceptOfferCommand request, CancellationToken cancellationToken)
        {
            var contract = await _context.Contracts
                .Include(c => c.Proposal)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException($"Contract with ID {request.ContractId} not found.");
            }

            if (contract.FreelancerId != request.FreelancerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: Only the freelancer can accept the offer.");
            }

            // State Guard — the contract must be in Draft status (awaiting freelancer acceptance)
            if (contract.Status != ContractStatus.Draft)
            {
                throw new InvalidStateException("Only draft contracts can be accepted.");
            }

            // Also ensure the underlying proposal is in the right state
            ContractStateGuard.EnsureCanAcceptOffer(contract.Proposal);

            contract.Status = ContractStatus.Active;
            contract.AcceptedAt = DateTime.UtcNow;

            // Mark the proposal as Offer (accepted)
            contract.Proposal.Status = ProposalStatus.Offer;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
