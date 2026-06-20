using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Services.Wallet;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class ResolveDisputeCommandHandler : IRequestHandler<ResolveDisputeCommand, DisputeDto>
    {
        private readonly AppDbContext _context;
        private readonly IEscrowService _escrowService;

        public ResolveDisputeCommandHandler(AppDbContext context, IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
        }

        public async Task<DisputeDto> Handle(ResolveDisputeCommand request, CancellationToken cancellationToken)
        {
            var dispute = await _context.Disputes
                .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

            if (dispute == null)
            {
                throw new NotFoundException($"Dispute with ID {request.DisputeId} not found.");
            }

            if (dispute.Status != DisputeStatus.Open && dispute.Status != DisputeStatus.UnderReview)
            {
                throw new InvalidStateException("Dispute is already resolved.");
            }

            var delivery = await _context.ContractDeliveries
                .FirstOrDefaultAsync(d => d.Id == dispute.ContractDeliveryId, cancellationToken);

            if (delivery == null)
            {
                throw new NotFoundException("Associated delivery not found.");
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException("Associated contract not found.");
            }

            var contractGuid = Guid.Parse($"00000000-0000-0000-0000-{contract.Id:x12}");

            decimal clientPct = 0;
            decimal freelancerPct = 0;

            if (request.ClientPercentage.HasValue && request.FreelancerPercentage.HasValue)
            {
                clientPct = request.ClientPercentage.Value;
                freelancerPct = request.FreelancerPercentage.Value;

                if (clientPct < 0 || clientPct > 100 || freelancerPct < 0 || freelancerPct > 100)
                {
                    throw new ValidationException("Percentages must be between 0 and 100.");
                }

                if (clientPct + freelancerPct != 100m)
                {
                    throw new ValidationException("Percentages must sum to exactly 100.");
                }
            }
            else if (request.Decision.HasValue)
            {
                if (request.Decision == DisputeDecision.ForFreelancer)
                {
                    clientPct = 0;
                    freelancerPct = 100;
                }
                else // ForClient
                {
                    clientPct = 100;
                    freelancerPct = 0;
                }
            }
            else
            {
                throw new ValidationException("Either percentages or a decision must be provided.");
            }

            await _escrowService.ResolveDisputeSplitAsync(contractGuid, delivery.ContractMilestoneId, clientPct, freelancerPct, request.AdminDecision);

            if (clientPct == 100m)
            {
                dispute.Status = DisputeStatus.ResolvedForClient;
            }
            else if (freelancerPct == 100m)
            {
                dispute.Status = DisputeStatus.ResolvedForFreelancer;
            }
            else
            {
                dispute.Status = DisputeStatus.ResolvedSplit;
            }

            dispute.ClientPercentage = clientPct;
            dispute.FreelancerPercentage = freelancerPct;

            if (freelancerPct > 0)
            {
                delivery.Status = DeliveryStatus.Approved;
                delivery.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                delivery.Status = DeliveryStatus.Pending;
            }

            // Close the contract
            contract.Status = ContractStatus.Terminated;
            contract.ClosedAt = DateTime.UtcNow;

            // Close the associated proposal
            if (contract.ProposalId.HasValue)
            {
                var proposal = await _context.Proposals
                    .FirstOrDefaultAsync(p => p.Id == contract.ProposalId.Value, cancellationToken);
                if (proposal != null)
                {
                    proposal.Status = ProposalStatus.Rejected;
                }
            }

            dispute.AdminId = request.AdminId;
            dispute.AdminDecision = request.AdminDecision;
            dispute.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return dispute.ToDto();
        }
    }
}
