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

namespace ServiceImplementation.Implementations.Reviews
{
    public class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, ContractReviewReadDTO>
    {
        private readonly AppDbContext _context;

        public SubmitReviewCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractReviewReadDTO> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
        {
            var contract = await _context.Contracts
                .Include(c => c.WorkDeliveries)
                .Include(c => c.ContractReviews)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException($"Contract with ID {request.ContractId} not found.");
            }

            var errors = new System.Collections.Generic.List<string>();
            if (request.Dto.Rating < 1 || request.Dto.Rating > 5)
            {
                errors.Add("Rating must be between 1 and 5.");
            }
            if (errors.Any())
            {
                throw new ValidationException("Validation failed", errors);
            }

            // Check if reviewer is part of the contract
            if (contract.ClientId != request.ReviewerId && contract.FreelancerId != request.ReviewerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: Only clients or freelancers associated with the contract can submit a review.");
            }

            // State Guard
            ContractStateGuard.EnsureCanSubmitReview(contract, request.ReviewerId);

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
            // We check the existing reviews plus the one being added.
            bool clientReviewExists = contract.ContractReviews.Any(r => r.ReviewerId == contract.ClientId) || request.ReviewerId == contract.ClientId;
            bool freelancerReviewExists = contract.ContractReviews.Any(r => r.ReviewerId == contract.FreelancerId) || request.ReviewerId == contract.FreelancerId;

            if (clientReviewExists && freelancerReviewExists)
            {
                contract.Status = ContractStatus.Completed;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new ContractReviewReadDTO
            {
                Id = review.Id,
                ContractId = review.ContractId,
                ReviewerId = review.ReviewerId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
