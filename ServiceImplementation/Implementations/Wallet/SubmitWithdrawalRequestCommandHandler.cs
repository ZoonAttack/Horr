using MediatR;
using Entities;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Exceptions;
using Microsoft.EntityFrameworkCore;
using Entities.Enums;

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

            // Implementation will follow in next subtasks
            throw new NotImplementedException();
        }

        private async Task ValidateAsync(SubmitWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();

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

            // Check balance
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
