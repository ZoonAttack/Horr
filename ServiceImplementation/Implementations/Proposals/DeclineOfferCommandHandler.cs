using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Proposals
{
    public class DeclineOfferCommandHandler : IRequestHandler<DeclineOfferCommand, Unit>
    {
        private readonly AppDbContext _context;

        public DeclineOfferCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeclineOfferCommand request, CancellationToken cancellationToken)
        {
            var proposal = await _context.Proposals
                .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

            if (proposal == null)
            {
                throw new NotFoundException($"Proposal with ID {request.ProposalId} not found.");
            }

            if (proposal.FreelancerId != request.FreelancerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: You can only decline offers sent to you.");
            }

            // Implementation per EARS: set Proposal.Status = Rejected without deleting any record
            proposal.Status = ProposalStatus.Rejected;

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
