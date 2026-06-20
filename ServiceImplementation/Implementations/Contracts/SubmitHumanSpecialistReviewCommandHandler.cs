using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using Entities.Project;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class SubmitHumanSpecialistReviewCommandHandler : IRequestHandler<SubmitHumanSpecialistReviewCommand, ContractSpecialistReviewReadDto>
    {
        private readonly AppDbContext _context;

        public SubmitHumanSpecialistReviewCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractSpecialistReviewReadDto> Handle(SubmitHumanSpecialistReviewCommand request, CancellationToken cancellationToken)
        {
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
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
            {
                throw new NotFoundException($"Specialist review with ID {request.ReviewId} not found.");
            }

            if (review.AssignedSpecialistId != request.SpecialistId)
            {
                throw new ForbiddenException("You are not authorized to submit a decision for this review.");
            }

            if (review.ReviewerType != ReviewerType.Human)
            {
                throw new InvalidStateException("AI reviews cannot be submitted manually.");
            }

            if (review.Status != SpecialistReviewStatus.InProgress)
            {
                throw new InvalidStateException("This review request is not in progress.");
            }

            review.Verdict = request.Verdict;
            review.ReviewNote = request.ReviewNote;
            review.Status = SpecialistReviewStatus.Completed;
            review.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return review.ToReadDto();
        }
    }
}
