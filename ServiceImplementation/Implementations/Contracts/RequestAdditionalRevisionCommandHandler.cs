using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Helpers;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public class RequestAdditionalRevisionCommandHandler : IRequestHandler<RequestAdditionalRevisionCommand, Result<AdditionalRevisionRequestDto>>
    {
        private readonly AppDbContext _context;

        public RequestAdditionalRevisionCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<AdditionalRevisionRequestDto>> Handle(RequestAdditionalRevisionCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _context.ContractDeliveries
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if (delivery == null)
            {
                return new Result<AdditionalRevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AttachmentNotFound,
                    Message = $"Delivery with ID {request.DeliveryId} not found."
                };
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == delivery.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<AdditionalRevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = "Associated contract not found."
                };
            }

            if (contract.ClientId != request.ClientId)
            {
                return new Result<AdditionalRevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Only the contract client can request additional revisions."
                };
            }

            if (request.RequestedCount <= 0)
            {
                return new Result<AdditionalRevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidAmount,
                    Message = "Requested revision count must be greater than 0."
                };
            }

            // Verify they have indeed reached or exceeded the revision limit
            var currentRevisionCount = await _context.RevisionRequests
                .CountAsync(rr => rr.Delivery.ContractId == contract.Id, cancellationToken);

            if (currentRevisionCount < contract.MaxRevisions)
            {
                return new Result<AdditionalRevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = $"Cannot request additional revisions yet. You have only used {currentRevisionCount} out of {contract.MaxRevisions} revisions."
                };
            }

            // Check if there is already a pending additional revision request for this delivery
            var pendingRequestExists = await _context.AdditionalRevisionRequests
                .AnyAsync(r => r.DeliveryId == delivery.Id && r.Status == RequestStatus.Pending, cancellationToken);

            if (pendingRequestExists)
            {
                return new Result<AdditionalRevisionRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "An additional revision request is already pending for this delivery."
                };
            }

            var additionalRequest = new AdditionalRevisionRequest
            {
                ContractId = contract.Id,
                DeliveryId = delivery.Id,
                RequestedCount = request.RequestedCount,
                ClientId = request.ClientId,
                Reason = request.Reason,
                Status = RequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.AdditionalRevisionRequests.Add(additionalRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result<AdditionalRevisionRequestDto>
            {
                Succeeded = true,
                Data = new AdditionalRevisionRequestDto
                {
                    Id = additionalRequest.Id,
                    ContractId = additionalRequest.ContractId,
                    DeliveryId = additionalRequest.DeliveryId,
                    RequestedCount = additionalRequest.RequestedCount,
                    ClientId = additionalRequest.ClientId,
                    ClientName = contract.Client.FullName,
                    Reason = additionalRequest.Reason,
                    Status = additionalRequest.Status,
                    RequestedAt = additionalRequest.RequestedAt
                }
            };
        }
    }
}