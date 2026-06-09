using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Services.Wallet;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class FundMilestoneCommandHandler : IRequestHandler<FundMilestoneCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IEscrowService _escrowService;

        public FundMilestoneCommandHandler(AppDbContext context, IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
        }

        public async Task<bool> Handle(FundMilestoneCommand request, CancellationToken cancellationToken)
        {
            var milestone = await _context.ContractMilestones
                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

            if (milestone == null)
            {
                throw new NotFoundException($"Milestone with ID {request.MilestoneId} not found.");
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException("Associated contract not found.");
            }

            // Who: Client only
            if (contract.ClientId != request.ClientId.ToString())
            {
                throw new ForbiddenException("Only the contract client can fund the milestone.");
            }

            await _escrowService.FundMilestoneAsync(request.MilestoneId, request.ClientId);
            return true;
        }
    }
}
