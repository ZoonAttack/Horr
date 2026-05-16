using MediatR;
using Entities;
using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ServiceImplementation.Implementations.Wallet
{
    public class SubmitWithdrawalRequestCommandHandler : IRequestHandler<SubmitWithdrawalRequestCommand, WithdrawalRequestDto>
    {
        private readonly AppDbContext _context;

        public SubmitWithdrawalRequestCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WithdrawalRequestDto> Handle(SubmitWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, cancellationToken);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var wallet = await _context.WalletBalances
                    .FirstOrDefaultAsync(w => w.UserId == request.FreelancerId, cancellationToken);

                // Re-check balance inside transaction
                if (wallet == null || wallet.BalanceEGP < request.Amount)
                {
                    throw new ValidationException("Insufficient wallet balance.");
                }

                var withdrawalRequest = new WithdrawalRequest
                {
                    FreelancerId = request.FreelancerId,
                    Amount = request.Amount,
                    Method = request.Method,
                    InstapayUsername = request.InstapayUsername,
                    BankAccountDetails = request.BankAccountDetails,
                    EWalletNumber = request.EWalletNumber,
                    Status = WithdrawalStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                };

                _context.WithdrawalRequests.Add(withdrawalRequest);

                // Deduct balance (hold funds)
                wallet.BalanceEGP -= request.Amount;
                wallet.LastUpdatedAt = DateTime.UtcNow;

                // Add transaction record
                var financialTransaction = new Transaction
                {
                    UserId = request.FreelancerId,
                    Amount = request.Amount,
                    Direction = TransactionDirection.Debit,
                    Type = TransactionType.Withdrawal,
                    Description = $"Withdrawal Request Submitted: {request.Method}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Transactions.Add(financialTransaction);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return withdrawalRequest.ToDto();
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task ValidateAsync(SubmitWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            if (request.Amount <= 0)
            {
                errors.Add("Amount must be greater than zero.");
            }

            if (request.Method == WithdrawalMethod.InstaPay && string.IsNullOrWhiteSpace(request.InstapayUsername))
            {
                errors.Add("InstaPay username is required.");
            }

            if (request.Method == WithdrawalMethod.BankTransfer && string.IsNullOrWhiteSpace(request.BankAccountDetails))
            {
                errors.Add("Bank account details are required.");
            }

            if (request.Method == WithdrawalMethod.EWallet && string.IsNullOrWhiteSpace(request.EWalletNumber))
            {
                errors.Add("E-wallet number is required.");
            }

            var wallet = await _context.WalletBalances
                .FirstOrDefaultAsync(w => w.UserId == request.FreelancerId, cancellationToken);
            
            decimal balance = wallet?.BalanceEGP ?? 0;

            if (request.Amount > balance)
            {
                errors.Add("Insufficient wallet balance.");
            }

            if (errors.Any())
            {
                throw new ValidationException("Validation failed", errors);
            }
        }
    }
}
