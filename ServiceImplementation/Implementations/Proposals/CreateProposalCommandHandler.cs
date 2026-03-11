using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using ServiceContracts.DTOs.Proposal;
using ServiceImplementation.Exceptions;
using System.Text.RegularExpressions;

namespace ServiceImplementation.Implementations.Proposals
{
    public class CreateProposalCommandHandler : IRequestHandler<CreateProposalCommand, ProposalReadDTO>
    {
        private readonly AppDbContext _context;

        public CreateProposalCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProposalReadDTO> Handle(CreateProposalCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // 1. Domain Validation
            Validate(dto);

            // 2. Verify user is a registered freelancer
            var freelancerExists = await _context.Freelancers
                .AnyAsync(f => f.UserId == request.FreelancerId, cancellationToken);

            if (!freelancerExists)
            {
                throw new NotFoundException("You are not registered as a freelancer. Please complete your freelancer profile first.");
            }

            // 3. Check for duplicate proposal
            var exists = await _context.Proposals
                .AnyAsync(p => p.FreelancerId == request.FreelancerId && p.JobPostId == dto.JobPostId, cancellationToken);

            if (exists)
            {
                throw new ConflictException("You have already submitted a proposal for this job post.");
            }

            // 4. Verify job post exists
            var job = await _context.JobPosts.FindAsync(new object[] { dto.JobPostId }, cancellationToken);
            if (job == null)
            {
                throw new NotFoundException($"Job post with ID {dto.JobPostId} not found.");
            }

            // 4. Calculate HORR Fee
            decimal horrFee = Math.Round(dto.BidRate * 0.10m, 2);

            // 5. Create entity
            var proposal = new Proposal
            {
                JobPostId = dto.JobPostId,
                FreelancerId = request.FreelancerId,
                SubmitAsType = dto.SubmitAsType,
                BidRate = dto.BidRate,
                HORRFee = horrFee,
                CoverLetter = dto.CoverLetter,
                CreatedAt = DateTime.UtcNow,
                Terms = dto.Terms.Select(t => new ProposalTerm
                {
                    MilestoneTitle = t.MilestoneTitle,
                    Amount = t.Amount,
                    DueDate = t.DueDate
                }).ToList()
            };

            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync(cancellationToken);

            return new ProposalReadDTO
            {
                Id = proposal.Id,
                JobPostId = proposal.JobPostId,
                JobPostTitle = job.Title,
                FreelancerId = proposal.FreelancerId,
                SubmitAsType = proposal.SubmitAsType,
                BidRate = proposal.BidRate,
                HORRFee = proposal.HORRFee,
                CoverLetter = proposal.CoverLetter,
                Status = proposal.Status,
                CreatedAt = proposal.CreatedAt,
                Terms = proposal.Terms.Select(t => new ProposalTermReadDTO
                {
                    Id = t.Id,
                    MilestoneTitle = t.MilestoneTitle,
                    Amount = t.Amount,
                    DueDate = t.DueDate
                }).ToList()
            };
        }

        private void Validate(ProposalCreateDTO dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.CoverLetter))
            {
                errors.Add("CoverLetter is required.");
            }
            else
            {
                if (dto.CoverLetter.Length < 50)
                    errors.Add("CoverLetter must be at least 50 characters.");
                
                if (dto.CoverLetter.Length > 2000)
                    errors.Add("CoverLetter cannot exceed 2000 characters.");

                if (!Regex.IsMatch(dto.CoverLetter, @"^[\u0600-\u06FFa-zA-Z0-9\s\.,!?]+$"))
                    errors.Add("CoverLetter contains invalid characters.");
            }

            if (dto.BidRate <= 0)
            {
                errors.Add("BidRate must be greater than 0.");
            }

            if (errors.Any())
            {
                throw new ValidationException("Validation failed", errors);
            }
        }
    }
}
