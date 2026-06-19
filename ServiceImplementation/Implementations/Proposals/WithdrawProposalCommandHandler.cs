using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceImplementation.Exceptions;
using Entities.Enums;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Proposals
{
    public class WithdrawProposalCommandHandler : IRequestHandler<WithdrawProposalCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public WithdrawProposalCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(WithdrawProposalCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Freelancer account not found or is deleted."
                };
            }

            var proposal = await _context.Proposals
                .FirstOrDefaultAsync(p => p.Id == request.ProposalId && p.FreelancerId == request.FreelancerId, cancellationToken);

            if (proposal == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ProposalNotFound,
                    Message = $"Proposal with ID {request.ProposalId} not found or you are not authorized to withdraw it."
                };
            }

            proposal.Status = ProposalStatus.Withdrawn;
            await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
