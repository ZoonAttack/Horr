using MediatR;
using Entities;
using Services;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Wallet
{
    public class WalletQueryHandlers : 
        IRequestHandler<GetMyDepositRequestsQuery, PagedResult<DepositRequestDto>>,
        IRequestHandler<GetMyWithdrawalRequestsQuery, PagedResult<WithdrawalRequestDto>>,
        IRequestHandler<GetPendingDepositRequestsQuery, IEnumerable<DepositRequestDto>>,
        IRequestHandler<GetPendingWithdrawalRequestsQuery, IEnumerable<WithdrawalRequestDto>>,
        IRequestHandler<GetWalletBalanceQuery, WalletBalanceDto>
    {
        private readonly AppDbContext _context;

        public WalletQueryHandlers(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DepositRequestDto>> Handle(GetMyDepositRequestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DepositRequests
                .Where(r => r.ClientId == request.UserId)
                .OrderByDescending(r => r.SubmittedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<DepositRequestDto>
            {
                Items = items.Select(i => i.ToDto()),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<WithdrawalRequestDto>> Handle(GetMyWithdrawalRequestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.WithdrawalRequests
                .Where(r => r.FreelancerId == request.UserId)
                .OrderByDescending(r => r.SubmittedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<WithdrawalRequestDto>
            {
                Items = items.Select(i => i.ToDto()),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<IEnumerable<DepositRequestDto>> Handle(GetPendingDepositRequestsQuery request, CancellationToken cancellationToken)
        {
            var items = await _context.DepositRequests
                .Where(r => r.Status == DepositStatus.Pending)
                .ToListAsync(cancellationToken);

            return items.Select(i => i.ToDto());
        }

        public async Task<IEnumerable<WithdrawalRequestDto>> Handle(GetPendingWithdrawalRequestsQuery request, CancellationToken cancellationToken)
        {
            var items = await _context.WithdrawalRequests
                .Where(r => r.Status == WithdrawalStatus.Pending)
                .ToListAsync(cancellationToken);

            return items.Select(i => i.ToDto());
        }

        public async Task<WalletBalanceDto> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _context.WalletBalances
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet == null)
            {
                return new WalletBalanceDto { UserId = request.UserId, BalanceEGP = 0 };
            }

            return wallet.ToDto();
        }
    }
}
