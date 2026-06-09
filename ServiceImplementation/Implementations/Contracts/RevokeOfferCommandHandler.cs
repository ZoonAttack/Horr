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

        public RevokeOfferCommandHandler(AppDbContext context)
        {
            _context = context;
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
                    ErrorCode = "INVALID_STATE",
                    Message = "Only draft offers can be revoked."
                };
            }

            // Refund Escrowed Funds to Client
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
