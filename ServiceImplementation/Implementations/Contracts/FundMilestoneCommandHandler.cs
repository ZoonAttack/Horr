using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Services.Wallet;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class FundMilestoneCommandHandler : IRequestHandler<FundMilestoneCommand, Result<bool>>
    {
        private readonly AppDbContext _context;
        private readonly IEscrowService _escrowService;

        public FundMilestoneCommandHandler(AppDbContext context, IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
        }

        public async Task<Result<bool>> Handle(FundMilestoneCommand request, CancellationToken cancellationToken)
        {
            var milestone = await _context.ContractMilestones
                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

            if (milestone == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    Message = $"Milestone with ID {request.MilestoneId} not found.",
                    Errors = new List<string> { $"Milestone with ID {request.MilestoneId} not found." }
                };
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    Message = "Associated contract not found.",
                    Errors = new List<string> { "Associated contract not found." }
                };
            }

            // Who: Client only
            if (contract.ClientId != request.ClientId.ToString())
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    Message = "Only the contract client can fund the milestone.",
                    Errors = new List<string> { "Only the contract client can fund the milestone." }
                };
            }

            var fundResult = await _escrowService.FundMilestoneAsync(request.MilestoneId, request.ClientId);
            return fundResult;
        }
    }
}
