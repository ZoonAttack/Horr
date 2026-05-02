using Entities.Review;

namespace ServiceContracts.DTOs.Reviews
{
    public static class ReviewExtensions
    {
        public static ReviewDto ToDto(this ContractReview review)
        {
            if (review == null) return null!;

            return new ReviewDto
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
