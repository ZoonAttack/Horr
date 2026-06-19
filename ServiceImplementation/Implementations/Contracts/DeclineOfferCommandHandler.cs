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
using Entities.Payment;

namespace ServiceImplementation.Implementations.Contracts
{
    public class DeclineOfferCommandHandler : IRequestHandler<DeclineOfferCommand, Result<bool>>
    {
        private readonly AppDbContext _context;
        private readonly Services.Wallet.IEscrowService _escrowService;

        public DeclineOfferCommandHandler(AppDbContext context, Services.Wallet.IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
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
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "Only draft contracts can be declined."
                };
            }

            // Also ensure the underlying proposal is in the right state if it exists
            if (contract.Proposal != null)
            {
                ContractStateGuard.EnsureCanDeclineOffer(contract.Proposal);
                // Set proposal back to Rejected (not deleting any record per EARS)
                contract.Proposal.Status = ProposalStatus.Rejected;
            }

            // Mark contract as Rejected
            contract.Status = ContractStatus.Rejected;
            contract.RejectedAt = DateTime.UtcNow;
            contract.ClosedAt = DateTime.UtcNow;

            // Refund Escrowed Funds to Client via EscrowService
            var milestone = await _context.ContractMilestones
                .FirstOrDefaultAsync(m => m.ContractId == contract.Id, cancellationToken);
            if (milestone != null)
            {
                var contractGuid = Guid.Parse($"00000000-0000-0000-0000-{contract.Id:x12}");
                await _escrowService.RefundToClientAsync(contractGuid, milestone.Id, "Offer declined");
            }
            else
            {
                // Fallback for milestone-less legacy tests
                var clientWallet = await _context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == contract.ClientId, cancellationToken);
                if (clientWallet != null)
                {
                    clientWallet.BalanceEGP += contract.AgreedRate;
                    clientWallet.LastUpdatedAt = DateTime.UtcNow;

                    var transaction = new Transaction
                    {
                        UserId = contract.ClientId,
                        Amount = contract.AgreedRate,
                        Direction = TransactionDirection.Credit,
                        Type = TransactionType.Refund,
                        Description = $"Refund of escrowed funds for declined offer (Contract ID: {contract.Id})",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Transactions.Add(transaction);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
