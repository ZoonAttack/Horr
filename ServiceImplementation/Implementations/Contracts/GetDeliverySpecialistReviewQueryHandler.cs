using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetDeliverySpecialistReviewQueryHandler : IRequestHandler<GetDeliverySpecialistReviewQuery, Result<ContractSpecialistReviewReadDto>>
    {
        private readonly AppDbContext _context;

        public GetDeliverySpecialistReviewQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ContractSpecialistReviewReadDto>> Handle(GetDeliverySpecialistReviewQuery request, CancellationToken cancellationToken)
        {
            var delivery = await _context.ContractDeliveries
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Proposal)
                        .ThenInclude(p => p.JobPost)
                .Include(d => d.Contract)
                    .ThenInclude(c => c.JobPost)
                .Include(d => d.Attachments)
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if (delivery == null)
            {
                return new Result<ContractSpecialistReviewReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.DeliveryNotFound,
                    Message = $"Contract delivery with ID {request.DeliveryId} not found."
                };
            }

            if (delivery.Contract == null)
            {
                return new Result<ContractSpecialistReviewReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = "Associated contract not found."
                };
            }

            var review = await _context.ContractSpecialistReviews
                .Include(r => r.Delivery)
                    .ThenInclude(d => d.Contract)
                        .ThenInclude(c => c.Proposal)
                            .ThenInclude(p => p.JobPost)
                .Include(r => r.Delivery)
                    .ThenInclude(d => d.Contract)
                        .ThenInclude(c => c.JobPost)
                .Include(r => r.Delivery)
                    .ThenInclude(d => d.Attachments)
                .Where(r => r.DeliveryId == request.DeliveryId)
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var isAuthorized = delivery.Contract.ClientId == request.RequestingUserId ||
                               delivery.Contract.FreelancerId == request.RequestingUserId ||
                               (review != null && review.AssignedSpecialistId == request.RequestingUserId);

            if (!isAuthorized)
            {
                return new Result<ContractSpecialistReviewReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You are not authorized to view the specialist review for this delivery."
                };
            }

            if (review == null)
            {
                return new Result<ContractSpecialistReviewReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ReviewNotFound,
                    Message = "Specialist review not found for this delivery."
                };
            }

            return new Result<ContractSpecialistReviewReadDto>
            {
                Succeeded = true,
                Data = review.ToReadDto()
            };
        }
    }
}
