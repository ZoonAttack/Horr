using System;
using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Review;
using Entities.Enums;
using ServiceContracts.DTOs.Review;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Reviews
{
    public class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, Result<ContractReviewReadDTO>>
    {
        private readonly AppDbContext _context;

        public SubmitReviewCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ContractReviewReadDTO>> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ReviewerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<ContractReviewReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var contract = await _context.Contracts
                .Include(c => c.WorkDeliveries)
                .Include(c => c.ContractDeliveries)
                .Include(c => c.ContractReviews)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<ContractReviewReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = $"Contract with ID {request.ContractId} not found."
                };
            }

            if (request.Dto.Rating < 1 || request.Dto.Rating > 5)
            {
                return new Result<ContractReviewReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidRating,
                    Message = "Rating must be between 1 and 5."
                };
            }

            // Check if reviewer is part of the contract
            if (contract.ClientId != request.ReviewerId && contract.FreelancerId != request.ReviewerId)
            {
                return new Result<ContractReviewReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Unauthorized: Only clients or freelancers associated with the contract can submit a review."
                };
            }

            // State Guard
            try 
            {
                ContractStateGuard.EnsureCanSubmitReview(contract, request.ReviewerId);
            }
            catch (ConflictException ex)
            {
                return new Result<ContractReviewReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AlreadyReviewed,
                    Message = ex.Message
                };
            }
            catch (InvalidStateException ex)
            {
                return new Result<ContractReviewReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new Result<ContractReviewReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = ex.Message
                };
            }

            // Create review
            var review = new ContractReview
            {
                ContractId = contract.Id,
                ReviewerId = request.ReviewerId,
                Rating = request.Dto.Rating,
                Comment = request.Dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.ContractReviews.Add(review);
            
            // Per EARS: Transition to Completed if both parties have reviewed
            bool clientReviewExists = contract.ContractReviews.Any(r => r.ReviewerId == contract.ClientId) || request.ReviewerId == contract.ClientId;
            bool freelancerReviewExists = contract.ContractReviews.Any(r => r.ReviewerId == contract.FreelancerId) || request.ReviewerId == contract.FreelancerId;

            if (clientReviewExists && freelancerReviewExists)
            {
                contract.Status = ContractStatus.Completed;
            }

            await _context.SaveChangesAsync(cancellationToken);

            var dto = new ContractReviewReadDTO
            {
                Id = review.Id,
                ContractId = review.ContractId,
                ReviewerId = review.ReviewerId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };

            return new Result<ContractReviewReadDTO> { Succeeded = true, Data = dto };
        }
    }
}
