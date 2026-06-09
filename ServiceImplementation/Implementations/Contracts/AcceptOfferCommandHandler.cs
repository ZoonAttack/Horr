using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Entities.Communication;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceImplementation.Exceptions;
using Entities.Payment;

namespace ServiceImplementation.Implementations.Contracts
{
    public class AcceptOfferCommandHandler : IRequestHandler<AcceptOfferCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public AcceptOfferCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(AcceptOfferCommand request, CancellationToken cancellationToken)
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
                    Message = "Unauthorized: Only the freelancer can accept the offer."
                };
            }

            // State Guard — the contract must be in Draft status (awaiting freelancer acceptance)
            if (contract.Status != ContractStatus.Draft)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = "INVALID_STATE",
                    Message = "Only draft contracts can be accepted."
                };
            }

            // Also ensure the underlying proposal is in the right state
            ContractStateGuard.EnsureCanAcceptOffer(contract.Proposal);

            contract.Status = ContractStatus.Active;
            contract.AcceptedAt = DateTime.UtcNow;

            // Mark the proposal as Offer (accepted)
            contract.Proposal.Status = ProposalStatus.Offer;

            // Automatically create Chat room for the active contract if not exists
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
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
