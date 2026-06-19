using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using Entities.Payment;

namespace ServiceImplementation.Implementations.Contracts
{
    public class RevokeOfferCommandHandler : IRequestHandler<RevokeOfferCommand, Result<bool>>
    {
        private readonly AppDbContext _context;
        private readonly Services.Wallet.IEscrowService _escrowService;

        public RevokeOfferCommandHandler(AppDbContext context, Services.Wallet.IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
        }

        public async Task<Result<bool>> Handle(RevokeOfferCommand request, CancellationToken cancellationToken)
        {
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

            if (contract.ClientId != request.ClientId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Unauthorized: Only the client who created the offer can revoke it."
                };
            }

            if (contract.Status != ContractStatus.Draft)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "Only draft offers can be revoked."
                };
            }

            // Refund Escrowed Funds to Client via EscrowService
            var milestone = await _context.ContractMilestones
                .FirstOrDefaultAsync(m => m.ContractId == contract.Id, cancellationToken);
            if (milestone != null)
            {
                var contractGuid = Guid.Parse($"00000000-0000-0000-0000-{contract.Id:x12}");
                await _escrowService.RefundToClientAsync(contractGuid, milestone.Id, "Offer revoked");
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
                        Description = $"Refund of escrowed funds for revoked offer (Contract ID: {contract.Id})",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Transactions.Add(transaction);
                }
            }

            // Set proposal back to Rejected (or could be set to Submitted if allowing re-offer, but Rejected is safer)
            if (contract.Proposal != null)
            {
                contract.Proposal.Status = ProposalStatus.Rejected;
            }

            // Mark contract as Closed/Terminated
            contract.Status = ContractStatus.Closed;
            contract.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
