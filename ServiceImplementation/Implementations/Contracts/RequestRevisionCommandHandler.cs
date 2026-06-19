using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;
using Services;

namespace ServiceImplementation.Implementations.Contracts
{
    public class RequestRevisionCommandHandler : IRequestHandler<RequestRevisionCommand, Result<RevisionRequestDto>>
    {
        private readonly AppDbContext _context;

        public RequestRevisionCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<RevisionRequestDto>> Handle(RequestRevisionCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _context.ContractDeliveries
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if (delivery == null)
            {
                return new Result<RevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AttachmentNotFound,
                    Message = $"Delivery with ID {request.DeliveryId} not found."
                };
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == delivery.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<RevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = "Associated contract not found."
                };
            }

            // Who: Client only
            if (contract.ClientId != request.ClientId)
            {
                return new Result<RevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Only the contract client can request a revision."
                };
            }

            if (delivery.Status != DeliveryStatus.Pending)
            {
                return new Result<RevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "Revision can only be requested for pending deliveries."
                };
            }

            // Validation: Count current revisions on this contract
            var currentRevisionCount = await _context.RevisionRequests
                .CountAsync(rr => rr.Delivery.ContractId == contract.Id, cancellationToken);

            if (currentRevisionCount >= contract.MaxRevisions)
            {
                return new Result<RevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.RevisionLimitExceeded,
                    Message = $"You have reached the maximum allowed revisions ({contract.MaxRevisions}) for this contract. Please request additional revisions from the freelancer."
                };
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

            return new Result<RevisionRequestDto>
            {
                Succeeded = true,
                Data = revRequest.ToDto()
            };
        }
    }
}
