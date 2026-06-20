using MediatR;
using Entities;
using Services;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;
using Entities.Enums;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Wallet
{
    public class WalletQueryHandlers : 
        IRequestHandler<GetMyDepositRequestsQuery, Result<PagedResult<DepositRequestDto>>>,
        IRequestHandler<GetMyWithdrawalRequestsQuery, Result<PagedResult<WithdrawalRequestDto>>>,
        IRequestHandler<GetPendingDepositRequestsQuery, Result<IEnumerable<DepositRequestDto>>>,
        IRequestHandler<GetPendingWithdrawalRequestsQuery, Result<IEnumerable<WithdrawalRequestDto>>>,
        IRequestHandler<GetWalletBalanceQuery, Result<WalletBalanceDto>>
    {
        private readonly AppDbContext _context;

        public WalletQueryHandlers(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<DepositRequestDto>>> Handle(GetMyDepositRequestsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<PagedResult<DepositRequestDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var query = _context.DepositRequests
                .Where(r => r.ClientId == request.UserId)
                .OrderByDescending(r => r.SubmittedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new Result<PagedResult<DepositRequestDto>>
            {
                Succeeded = true,
                Data = new PagedResult<DepositRequestDto>
                {
                    Items = items.Select(i => i.ToDto()),
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                }
            };
        }

        public async Task<Result<PagedResult<WithdrawalRequestDto>>> Handle(GetMyWithdrawalRequestsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<PagedResult<WithdrawalRequestDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var query = _context.WithdrawalRequests
                .Include(r => r.Freelancer)
                .Where(r => r.FreelancerId == request.UserId)
                .OrderByDescending(r => r.SubmittedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new Result<PagedResult<WithdrawalRequestDto>>
            {
                Succeeded = true,
                Data = new PagedResult<WithdrawalRequestDto>
                {
                    Items = items.Select(i => i.ToDto()),
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                }
            };
        }

        public async Task<Result<IEnumerable<DepositRequestDto>>> Handle(GetPendingDepositRequestsQuery request, CancellationToken cancellationToken)
        {
            var items = await _context.DepositRequests
                .Where(r => r.Status == DepositStatus.Pending)
                .ToListAsync(cancellationToken);

            return new Result<IEnumerable<DepositRequestDto>>
            {
                Succeeded = true,
                Data = items.Select(i => i.ToDto())
            };
        }

        public async Task<Result<IEnumerable<WithdrawalRequestDto>>> Handle(GetPendingWithdrawalRequestsQuery request, CancellationToken cancellationToken)
        {
            var items = await _context.WithdrawalRequests
                .Include(r => r.Freelancer)
                .Where(r => r.Status == WithdrawalStatus.Pending)
                .ToListAsync(cancellationToken);

            return new Result<IEnumerable<WithdrawalRequestDto>>
            {
                Succeeded = true,
                Data = items.Select(i => i.ToDto())
            };
        }

        public async Task<Result<WalletBalanceDto>> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<WalletBalanceDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var wallet = await _context.WalletBalances
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet == null)
            {
                return new Result<WalletBalanceDto>
                {
                    Succeeded = true,
                    Data = new WalletBalanceDto { UserId = request.UserId, BalanceEGP = 0 }
                };
            }

            return new Result<WalletBalanceDto>
            {
                Succeeded = true,
                Data = wallet.ToDto()
            };
        }
    }
}
