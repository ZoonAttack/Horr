using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceImplementation.Exceptions;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Proposals
{
    public class WithdrawProposalCommandHandler : IRequestHandler<WithdrawProposalCommand>
    {
        private readonly AppDbContext _context;

        public WithdrawProposalCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(WithdrawProposalCommand request, CancellationToken cancellationToken)
        {
            var proposal = await _context.Proposals
                .FirstOrDefaultAsync(p => p.Id == request.ProposalId && p.FreelancerId == request.FreelancerId, cancellationToken);

            if (proposal == null)
            {
                throw new NotFoundException($"Proposal with ID {request.ProposalId} not found or you are not authorized to withdraw it.");
            }

            proposal.Status = ProposalStatus.Withdrawn;
            
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
