using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using ServiceImplementation.Exceptions;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Proposals
{
    public class RejectProposalCommandHandler : IRequestHandler<RejectProposalCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public RejectProposalCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(RejectProposalCommand request, CancellationToken cancellationToken)
        {
            var proposal = await _context.Proposals
                .Include(p => p.JobPost)
                .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

            if (proposal == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound, // Matching Withdraw pattern
                    Message = $"Proposal with ID {request.ProposalId} not found."
                };
            }

            if (proposal.JobPost.ClientId != request.ClientId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Unauthorized: Only the client who posted the job can reject proposals."
                };
            }

            if (proposal.Status != ProposalStatus.Submitted && proposal.Status != ProposalStatus.Active)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "Proposal cannot be rejected in its current status."
                };
            }

            proposal.Status = ProposalStatus.Rejected;
            await _context.SaveChangesAsync(cancellationToken);

            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
