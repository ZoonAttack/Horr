using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class OpenDisputeCommandHandler : IRequestHandler<OpenDisputeCommand, DisputeDto>
    {
        private readonly AppDbContext _context;

        public OpenDisputeCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DisputeDto> Handle(OpenDisputeCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _context.ContractDeliveries
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if (delivery == null)
            {
                throw new NotFoundException($"Delivery with ID {request.DeliveryId} not found.");
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == delivery.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException("Associated contract not found.");
            }

            // Who: Client or Freelancer only
            if (contract.ClientId != request.OpenedByUserId && contract.FreelancerId != request.OpenedByUserId)
            {
                throw new ForbiddenException("Only the client or freelancer involved can open a dispute.");
            }

            // EARS: If a dispute is already open for this delivery, the system shall prevent opening a duplicate and return 409 Conflict
            var alreadyDisputed = await _context.Disputes
                .AnyAsync(d => d.ContractDeliveryId == request.DeliveryId && d.Status == DisputeStatus.Open, cancellationToken);

            if (alreadyDisputed)
            {
                throw new ConflictException("A dispute is already open for this delivery.");
            }

            // Create dispute record
            var dispute = new Dispute
            {
                ContractId = contract.Id,
                ContractDeliveryId = delivery.Id,
                OpenedByUserId = request.OpenedByUserId,
                Reason = request.Reason,
                OpenedAt = DateTime.UtcNow,
                Status = DisputeStatus.Open
            };

            // Set Delivery status
            delivery.Status = DeliveryStatus.Disputed;

            _context.Disputes.Add(dispute);

            await _context.SaveChangesAsync(cancellationToken);

            return dispute.ToDto();
        }
    }
}
