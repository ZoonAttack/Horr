using System;
using System.Linq;
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
    public class SubmitDeliveryCommandHandler : IRequestHandler<SubmitDeliveryCommand, ContractDeliveryDto>
    {
        private readonly AppDbContext _context;

        public SubmitDeliveryCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractDeliveryDto> Handle(SubmitDeliveryCommand request, CancellationToken cancellationToken)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException($"Contract with ID {request.ContractId} not found.");
            }

            // Who: Freelancer only
            if (contract.FreelancerId != request.FreelancerId)
            {
                throw new ForbiddenException("Only the contract freelancer can submit work.");
            }

            // EARS (guard): If the contract is not in an active state, the system shall prevent submission and return 403 Forbidden
            if (contract.Status != ContractStatus.Active)
            {
                throw new ForbiddenException("Cannot submit delivery on a contract that is not active.");
            }

            // EARS (guard): If the contract's escrow is not in Held status, the system shall prevent submission and return 400 Bad Request
            var hasHeldEscrow = await _context.EscrowTransactions
                .AnyAsync(e => e.ContractId == contract.Id 
                               && e.ContractMilestoneId == request.ContractMilestoneId
                               && e.Status == EscrowStatus.Held 
                               && e.Type == EscrowTransactionType.ClientFunded, cancellationToken);

            if (!hasHeldEscrow)
            {
                throw new ValidationException("Escrow funds must be in Held status to submit a delivery.");
            }

            var delivery = new ContractDelivery
            {
                ContractId = contract.Id,
                ContractMilestoneId = request.ContractMilestoneId,
                SubmittedAt = DateTime.UtcNow,
                DeliveryNote = request.DeliveryNote,
                Status = DeliveryStatus.Pending,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            _context.ContractDeliveries.Add(delivery);

            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var attDto in request.Attachments)
                {
                    var attachment = attDto.ToEntity();
                    attachment.DeliveryId = delivery.Id;
                    attachment.UploadedAt = DateTime.UtcNow;
                    _context.DeliveryAttachments.Add(attachment);
                }
            }

            // Automatically resolve any pending revision requests on this contract
            var pendingRevisions = await _context.RevisionRequests
                .Where(r => r.Delivery.ContractId == contract.Id && r.Status == RevisionStatus.Pending)
                .ToListAsync(cancellationToken);

            foreach (var rev in pendingRevisions)
            {
                rev.Status = RevisionStatus.Resolved;
                rev.ResolvedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Re-fetch with attachments to map fully
            var deliveryWithAttachments = await _context.ContractDeliveries
                .Include(d => d.Attachments)
                .Include(d => d.RevisionRequests)
                .Include(d => d.AdditionalRevisionRequests)
                    .ThenInclude(arr => arr.Client)
                .FirstAsync(d => d.Id == delivery.Id, cancellationToken);

            return deliveryWithAttachments.ToDto();
        }
    }
}
