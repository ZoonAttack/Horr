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

            if (request.Decision == DisputeDecision.ForFreelancer)
            {
                dispute.Status = DisputeStatus.ResolvedForFreelancer;
                delivery.Status = DeliveryStatus.Approved;
                delivery.CompletedAt = DateTime.UtcNow;

                await _escrowService.ReleaseToFreelancerAsync(contractGuid, delivery.ContractMilestoneId);
            }
            else // ForClient
            {
                dispute.Status = DisputeStatus.ResolvedForClient;
                // Work was rejected / dispute won by client
                delivery.Status = DeliveryStatus.Pending; // Or keep in disputed/rejected state

                await _escrowService.RefundToClientAsync(contractGuid, delivery.ContractMilestoneId, request.AdminDecision);
            }

            dispute.AdminId = request.AdminId;
            dispute.AdminDecision = request.AdminDecision;
            dispute.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return dispute.ToDto();
        }
    }
}
