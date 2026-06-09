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
using Entities.Payment;

namespace ServiceImplementation.Implementations.Contracts
{
    public class CreateDirectOfferCommandHandler : IRequestHandler<CreateDirectOfferCommand, Result<ContractDto>>
    {
        private readonly AppDbContext _context;
        private readonly Services.Wallet.IEscrowService _escrowService;

        public CreateDirectOfferCommandHandler(AppDbContext context, Services.Wallet.IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
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

            Proposal? proposal = null;
            if (request.ProposalId.HasValue)
            {
                proposal = await _context.Proposals.FirstOrDefaultAsync(p => p.Id == request.ProposalId.Value, cancellationToken);
                if (proposal == null)
                {
                    return new Result<ContractDto>
                    {
                        Succeeded = false,
                        ErrorCode = ErrorCodes.InvalidOfferParties,
                        Message = "Invalid proposal specified.",
                        Errors = new List<string> { "The provided proposal ID does not exist." }
                    };
                }
            }

            var totalAmount = request.AgreedRate ?? proposal?.BidRate ?? job.Budget;
            if (totalAmount <= 0)
            {
                return new Result<ContractDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidAmount,
                    Message = "Agreed rate must be greater than 0.",
                    Errors = new List<string> { "Agreed rate must be greater than 0." }
                };
            }

            // 2. Check Wallet Balance (including client 5.5% platform fee)
            var totalCharge = totalAmount + (totalAmount * 0.055m);
            var wallet = await _context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == request.ClientId, cancellationToken);
            if (wallet == null || wallet.BalanceEGP < totalCharge)
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
                ProposalId           = request.ProposalId,
                CustomJobDescription = string.IsNullOrWhiteSpace(request.CustomJobDescription) ? job.Description : request.CustomJobDescription,
                AgreedRate           = totalAmount,
                Status               = ContractStatus.Draft,
                StartedAt            = DateTime.UtcNow,
                CreatedAt            = DateTime.UtcNow
            };

            var milestone = new ContractMilestone
            {
                Id = Guid.NewGuid(),
                Contract = contract,
                Title = "Single Payment Milestone",
                Description = "Single payment milestone representing the entire contract budget.",
                Amount = totalAmount,
                DueDate = DateTime.UtcNow.AddDays(14),
                Status = MilestoneStatus.Unfunded
            };

            _context.Contracts.Add(contract);
            _context.ContractMilestones.Add(milestone);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Fund the milestone/escrow immediately
            if (Guid.TryParse(request.ClientId, out Guid clientGuid))
            {
                await _escrowService.FundMilestoneAsync(milestone.Id, clientGuid);
            }
            else
            {
                // Fallback for non-guid testing ids
                wallet.BalanceEGP -= totalCharge;
                wallet.LastUpdatedAt = DateTime.UtcNow;

                var transaction = new Transaction
                {
                    UserId = request.ClientId,
                    Amount = totalCharge,
                    Direction = TransactionDirection.Debit,
                    Type = TransactionType.Escrow,
                    Description = $"Funds held in escrow for Contract Offer (Fallback)",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Transactions.Add(transaction);

                var escrowTx = new EscrowTransaction
                {
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Type = EscrowTransactionType.ClientFunded,
                    Amount = totalAmount,
                    PlatformFeeFromClient = totalAmount * 0.055m,
                    PlatformFeeFromFreelancer = totalAmount * 0.15m,
                    NetToFreelancer = totalAmount * 0.85m,
                    Status = EscrowStatus.Held,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EscrowTransactions.Add(escrowTx);

                milestone.Status = MilestoneStatus.Funded;
                milestone.FundedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
            }

            return new Result<ContractDto>
            {
                Succeeded = true,
                Data = contract.ToDto()
            };
        }
    }
}
