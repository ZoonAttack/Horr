using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetEscrowSummaryQueryHandler : IRequestHandler<GetEscrowSummaryQuery, EscrowSummaryDto>
    {
        private readonly AppDbContext _context;

        public GetEscrowSummaryQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EscrowSummaryDto> Handle(GetEscrowSummaryQuery request, CancellationToken cancellationToken)
        {
            var txs = await _context.EscrowTransactions
                .Where(t => t.ContractId == request.ContractId)
                .ToListAsync(cancellationToken);

            var totalFunded = txs
                .Where(t => t.Type == EscrowTransactionType.ClientFunded)
                .Sum(t => t.Amount);

            var totalReleased = txs
                .Where(t => t.Type == EscrowTransactionType.ReleasedToFreelancer)
                .Sum(t => t.NetToFreelancer);

            var totalRefunded = txs
                .Where(t => t.Type == EscrowTransactionType.RefundedToClient)
                .Sum(t => t.Amount);

            var platformEarned = txs
                .Where(t => t.Type == EscrowTransactionType.ClientFunded && t.Status == EscrowStatus.Released)
                .Sum(t => t.PlatformFeeFromClient + t.PlatformFeeFromFreelancer)
                + txs
                .Where(t => t.Type == EscrowTransactionType.ClientFunded && t.Status == EscrowStatus.Split)
                .Sum(t => t.PlatformFeeFromClient + (t.PlatformFeeFromFreelancer * ((t.FreelancerPercentage ?? 0m) / 100m)));

            var currentlyHeld = txs
                .Where(t => t.Type == EscrowTransactionType.ClientFunded && t.Status == EscrowStatus.Held)
                .Sum(t => t.Amount);

            return new EscrowSummaryDto
            {
                TotalFunded = totalFunded,
                TotalReleased = totalReleased,
                TotalRefunded = totalRefunded,
                PlatformEarned = platformEarned,
                CurrentlyHeld = currentlyHeld
            };
        }
    }
}
