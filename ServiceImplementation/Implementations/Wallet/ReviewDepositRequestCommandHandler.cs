using MediatR;
using Entities;
using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ServiceImplementation.Implementations.Wallet
{
    public class ReviewDepositRequestCommandHandler : IRequestHandler<ReviewDepositRequestCommand, DepositRequestDto>
    {
        private readonly AppDbContext _context;

        public ReviewDepositRequestCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DepositRequestDto> Handle(ReviewDepositRequestCommand request, CancellationToken cancellationToken)
        {
            var depositRequest = await _context.DepositRequests
                .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

            if (depositRequest == null)
            {
                throw new NotFoundException("Deposit request not found.");
            }

            if (depositRequest.Status != DepositStatus.Pending)
            {
                throw new InvalidStateException("Only pending deposit requests can be reviewed.");
            }

            IDbContextTransaction? transaction = null;
            if (_context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            }
            
            try
            {
                depositRequest.Status = request.Status;
                depositRequest.ReviewedAt = DateTime.UtcNow;
                depositRequest.AdminNote = request.AdminNote;

                if (request.Status == DepositStatus.Approved)
                {
                    var wallet = await _context.WalletBalances
                        .FirstOrDefaultAsync(w => w.UserId == depositRequest.ClientId, cancellationToken);

                    if (wallet == null)
                    {
                        wallet = new WalletBalance { UserId = depositRequest.ClientId, BalanceEGP = 0 };
                        _context.WalletBalances.Add(wallet);
                    }

                    wallet.BalanceEGP += depositRequest.Amount;
                    wallet.LastUpdatedAt = DateTime.UtcNow;

                    var financialTransaction = new Transaction
                    {
                        UserId = depositRequest.ClientId,
                        Amount = depositRequest.Amount,
                        Direction = TransactionDirection.Credit,
                        Type = TransactionType.Deposit,
                        Description = $"Deposit Approved: {depositRequest.ReceiptNumber}",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Transactions.Add(financialTransaction);
                }

                await _context.SaveChangesAsync(cancellationToken);
                
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return depositRequest.ToDto();
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
}
