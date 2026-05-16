using MediatR;
using Entities;
using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ServiceImplementation.Implementations.Wallet
{
    public class ReviewWithdrawalRequestCommandHandler : IRequestHandler<ReviewWithdrawalRequestCommand, Result<WithdrawalRequestDto>>
    {
        private readonly AppDbContext _context;

        public ReviewWithdrawalRequestCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<WithdrawalRequestDto>> Handle(ReviewWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            var withdrawalRequest = await _context.WithdrawalRequests
                .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

            if (withdrawalRequest == null)
            {
                throw new NotFoundException("Withdrawal request not found.");
            }

            if (withdrawalRequest.Status != WithdrawalStatus.Pending)
            {
                throw new InvalidStateException("Only pending withdrawal requests can be reviewed.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                withdrawalRequest.Status = request.Status;
                withdrawalRequest.ReviewedAt = DateTime.UtcNow;
                withdrawalRequest.AdminNote = request.AdminNote;

                if (request.Status == WithdrawalStatus.Rejected)
                {
                    // Return funds to wallet
                    var wallet = await _context.WalletBalances
                        .FirstOrDefaultAsync(w => w.UserId == withdrawalRequest.FreelancerId, cancellationToken);

                    if (wallet != null)
                    {
                        wallet.BalanceEGP += withdrawalRequest.Amount;
                        wallet.LastUpdatedAt = DateTime.UtcNow;

                        // Add transaction record for refund
                        var financialTransaction = new Transaction
                        {
                            UserId = withdrawalRequest.FreelancerId,
                            Amount = withdrawalRequest.Amount,
                            Direction = TransactionDirection.Credit,
                            Type = TransactionType.Withdrawal,
                            Description = $"Withdrawal Rejected - Refund: {withdrawalRequest.Id}",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Transactions.Add(financialTransaction);
                    }
                }
                else if (request.Status == WithdrawalStatus.Approved)
                {
                    // Funds were already deducted on submission (held).
                    // We just record the finality here if needed, or just let it be.
                    var financialTransaction = new Transaction
                    {
                        UserId = withdrawalRequest.FreelancerId,
                        Amount = withdrawalRequest.Amount,
                        Direction = TransactionDirection.Debit,
                        Type = TransactionType.Withdrawal,
                        Description = $"Withdrawal Approved: {withdrawalRequest.Id}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Transactions.Add(financialTransaction);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

            return new Result<WithdrawalRequestDto> { Succeeded = true, Data = withdrawalRequest.ToDto() };
        }
    }
}
