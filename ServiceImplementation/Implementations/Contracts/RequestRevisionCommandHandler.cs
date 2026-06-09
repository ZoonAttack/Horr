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
    public class RequestRevisionCommandHandler : IRequestHandler<RequestRevisionCommand, RevisionRequestDto>
    {
        private readonly AppDbContext _context;

        public RequestRevisionCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RevisionRequestDto> Handle(RequestRevisionCommand request, CancellationToken cancellationToken)
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

            // Who: Client only
            if (contract.ClientId != request.ClientId)
            {
                throw new ForbiddenException("Only the contract client can request a revision.");
            }

            if (delivery.Status != DeliveryStatus.Pending)
            {
                throw new InvalidStateException("Revision can only be requested for pending deliveries.");
            }

            // Create revision request
            var revRequest = new RevisionRequest
            {
                DeliveryId = delivery.Id,
                RequestedByClientId = request.ClientId,
                Reason = request.Reason,
                RequestedAt = DateTime.UtcNow,
                Status = RevisionStatus.Pending
            };

            // Set Delivery status
            delivery.Status = DeliveryStatus.RevisionRequested;

            _context.RevisionRequests.Add(revRequest);

            await _context.SaveChangesAsync(cancellationToken);

            return revRequest.ToDto();
        }
    }
}
