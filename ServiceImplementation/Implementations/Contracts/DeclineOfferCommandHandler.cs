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
    public class DeclineOfferCommandHandler : IRequestHandler<DeclineOfferCommand, bool>
    {
        private readonly AppDbContext _context;

        public DeclineOfferCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeclineOfferCommand request, CancellationToken cancellationToken)
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
                throw new UnauthorizedAccessException("Unauthorized: Only the freelancer can decline the offer.");
            }

            // State Guard — keep the contract in Draft only
            if (contract.Status != ContractStatus.Draft)
            {
                throw new InvalidStateException("Only draft contracts can be declined.");
            }

            ContractStateGuard.EnsureCanDeclineOffer(contract.Proposal);

            // Set proposal back to Rejected (not deleting any record per EARS)
            contract.Proposal.Status = ProposalStatus.Rejected;

            // Mark contract as Rejected
            contract.Status = ContractStatus.Rejected;
            contract.RejectedAt = DateTime.UtcNow;
            contract.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
