using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Review;
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
