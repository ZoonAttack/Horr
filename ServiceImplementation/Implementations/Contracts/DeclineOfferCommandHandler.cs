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
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public class DeclineOfferCommandHandler : IRequestHandler<DeclineOfferCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public DeclineOfferCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeclineOfferCommand request, CancellationToken cancellationToken)
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

            var contract = await _context.Contracts
                .Include(c => c.Proposal)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = $"Contract with ID {request.ContractId} not found."
                };
            }

            if (contract.FreelancerId != request.FreelancerId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Unauthorized: Only the freelancer can decline the offer."
                };
            }

            // State Guard — keep the contract in Draft only
            if (contract.Status != ContractStatus.Draft)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = "INVALID_STATE",
                    Message = "Only draft contracts can be declined."
                };
            }

            ContractStateGuard.EnsureCanDeclineOffer(contract.Proposal);

            // Set proposal back to Rejected (not deleting any record per EARS)
            contract.Proposal.Status = ProposalStatus.Rejected;

            // Mark contract as Rejected
            contract.Status = ContractStatus.Rejected;
            contract.RejectedAt = DateTime.UtcNow;
            contract.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
