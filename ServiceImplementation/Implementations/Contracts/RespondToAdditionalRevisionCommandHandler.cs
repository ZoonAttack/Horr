using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class RespondToAdditionalRevisionCommandHandler : IRequestHandler<RespondToAdditionalRevisionCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public RespondToAdditionalRevisionCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(RespondToAdditionalRevisionCommand request, CancellationToken cancellationToken)
        {
            var additionalRequest = await _context.AdditionalRevisionRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

            if (additionalRequest == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvitationNotFound, // General not found
                    Message = $"Additional revision request with ID {request.RequestId} not found."
                };
            }

            if (additionalRequest.Contract.FreelancerId != request.FreelancerId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Only the contract freelancer can respond to this request."
                };
            }

            if (additionalRequest.Status != RequestStatus.Pending)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "This request has already been processed."
                };
            }

            if (request.Accept)
            {
                additionalRequest.Status = RequestStatus.Completed;
                // Increment contract revision cap
                additionalRequest.Contract.MaxRevisions += additionalRequest.RequestedCount;
            }
            else
            {
                additionalRequest.Status = RequestStatus.Failed;
            }

            additionalRequest.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new Result<bool>
            {
                Succeeded = true,
                Data = true
            };
        }
    }
}