using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Entities;
using Entities.Enums;
using Entities.Project;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using Services;

namespace ServiceImplementation.Implementations.Contracts
{
    public class CreateDirectOfferCommandHandler : IRequestHandler<CreateDirectOfferCommand, Result<ContractDto>>
    {
        private readonly AppDbContext _context;

        public CreateDirectOfferCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ContractDto>> Handle(CreateDirectOfferCommand request, CancellationToken cancellationToken)
        {
            // 1. Validation
            var client     = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId,     cancellationToken);
            var freelancer = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            var job        = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == request.JobPostId, cancellationToken);

            if (client == null || freelancer == null || job == null)
            {
                return new Result<ContractDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidOfferParties,
                    Message = "Invalid client, freelancer, or job specified.",
                    Errors = new List<string> { "One or more of the provided IDs does not exist." }
                };
            }

            if (request.Milestones == null || !request.Milestones.Any())
            {
                return new Result<ContractDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.MilestonesRequired,
                    Message = "Milestones are required.",
                    Errors = new List<string> { "An offer must contain at least one milestone." }
                };
            }

            var totalAmount = request.Milestones.Sum(m => m.Amount);

            // 2. Check Wallet Balance
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == request.ClientId, cancellationToken);
            if (wallet == null || wallet.Balance < totalAmount)
            {
                return new Result<ContractDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InsufficientBalance,
                    Message = "Insufficient wallet balance.",
                    Errors = new List<string> { "Please deposit funds before sending an offer." }
                };
            }

            // 3. Create Contract in Draft State
            var contract = new Contract
            {
                ClientId             = request.ClientId,
                FreelancerId         = request.FreelancerId,
                JobPostId            = request.JobPostId,
                CustomJobDescription = request.CustomJobDescription,
                AgreedRate           = totalAmount,
                Status               = ContractStatus.Draft,
                StartedAt            = DateTime.UtcNow,
                ContractMilestones   = request.Milestones.Select(m => new ContractMilestone
                {
                    Description = m.Title,
                    Amount      = m.Amount,
                    DueDate     = m.DueDate
                }).ToList()
            };

            _context.Contracts.Add(contract);

            // Funds are NOT deducted here — only verified.
            // Deduction happens when the freelancer accepts (escrow logic).
            await _context.SaveChangesAsync(cancellationToken);

            return new Result<ContractDto>
            {
                Succeeded = true,
                Data = contract.ToDto()
            };
        }
    }
}
