using MediatR;
using Entities;
using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Wallet
{
    public class SubmitWithdrawalRequestCommandHandler : IRequestHandler<SubmitWithdrawalRequestCommand, Result<WithdrawalRequestDto>>
    {
        private readonly AppDbContext _context;

        public SubmitWithdrawalRequestCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<WithdrawalRequestDto>> Handle(SubmitWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<WithdrawalRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Freelancer account not found or is deleted."
                };
            }

            var validationResult = await ValidateAsync(request, cancellationToken);
            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

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

                 return new Result<WithdrawalRequestDto>
            {
                Succeeded = true,
                Data = withdrawalRequest.ToDto()
            };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<Result<WithdrawalRequestDto>> ValidateAsync(SubmitWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
            {
                return new Result<WithdrawalRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.InvalidAmount, Message = "Amount must be greater than zero." };
            }

            if (request.Method == WithdrawalMethod.InstaPay && string.IsNullOrWhiteSpace(request.InstapayUsername))
            {
                return new Result<WithdrawalRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.MissingPaymentDetails, Message = "InstaPay username is required." };
            }

            if (request.Method == WithdrawalMethod.BankTransfer && string.IsNullOrWhiteSpace(request.BankAccountDetails))
            {
                return new Result<WithdrawalRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.MissingPaymentDetails, Message = "Bank account details are required." };
            }

            if (request.Method == WithdrawalMethod.EWallet && string.IsNullOrWhiteSpace(request.EWalletNumber))
            {
                return new Result<WithdrawalRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.MissingPaymentDetails, Message = "E-wallet number is required." };
            }

            var wallet = await _context.WalletBalances
                .FirstOrDefaultAsync(w => w.UserId == request.FreelancerId, cancellationToken);
            
            decimal balance = wallet?.BalanceEGP ?? 0;

            if (request.Amount > balance)
            {
                return new Result<WithdrawalRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.InsufficientBalance, Message = "Insufficient wallet balance." };
            }

            return new Result<WithdrawalRequestDto> { Succeeded = true };
        }
    }
}
