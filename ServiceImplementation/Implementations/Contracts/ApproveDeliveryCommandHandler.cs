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
    public class ApproveDeliveryCommandHandler : IRequestHandler<ApproveDeliveryCommand, ContractDeliveryDto>
    {
        private readonly AppDbContext _context;
        private readonly IEscrowService _escrowService;

        public ApproveDeliveryCommandHandler(AppDbContext context, IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
        }

        public async Task<ContractDeliveryDto> Handle(ApproveDeliveryCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _context.ContractDeliveries
                .Include(d => d.Attachments)
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if (delivery == null)
            {
                throw new NotFoundException($"Delivery with ID {request.DeliveryId} not found.");
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == delivery.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException($"Associated contract not found.");
            }

            // Who: Client only
            if (contract.ClientId != request.ClientId)
            {
                throw new ForbiddenException("Only the contract client can approve the delivery.");
            }

            if (delivery.Status != DeliveryStatus.Pending)
            {
                throw new InvalidStateException("Only pending deliveries can be approved.");
            }

            // Update status to Approved
            delivery.Status = DeliveryStatus.Approved;
            delivery.CompletedAt = DateTime.UtcNow;

            // Release escrow funds to freelancer
            var contractGuid = Guid.Parse($"00000000-0000-0000-0000-{contract.Id:x12}");

            await _escrowService.ReleaseToFreelancerAsync(contractGuid, delivery.ContractMilestoneId);

            // Auto-complete and close the contract
            contract.Status = ContractStatus.Completed;
            contract.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return delivery.ToDto();
        }
    }
}
