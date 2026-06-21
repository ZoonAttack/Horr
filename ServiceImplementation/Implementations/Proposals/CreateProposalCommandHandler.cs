using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using Entities.Project;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Proposal;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;
using System.Text.RegularExpressions;

namespace ServiceImplementation.Implementations.Proposals
{
    public class CreateProposalCommandHandler : IRequestHandler<CreateProposalCommand, Result<ProposalReadDTO>>
    {
        private readonly AppDbContext _context;

        public CreateProposalCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ProposalReadDTO>> Handle(CreateProposalCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // 1. Verify user is not deleted
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<ProposalReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Freelancer account not found or is deleted."
                };
            }

            // 2. Domain Validation
            var validationErrors = Validate(dto);
            if (validationErrors.Any())
            {
                return new Result<ProposalReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "Validation failed",
                    Errors = validationErrors
                };
            }

            // 3. Verify user is a registered freelancer
            var freelancerExists = await _context.Freelancers
                .AnyAsync(f => f.UserId == request.FreelancerId, cancellationToken);

            if (!freelancerExists)
            {
                return new Result<ProposalReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FreelancerNotFound,
                    Message = "You are not registered as a freelancer. Please complete your freelancer profile first."
                };
            }

            // 4. Check for duplicate proposal
            var exists = await _context.Proposals
                .AnyAsync(p => p.FreelancerId == request.FreelancerId 
                            && (p.Status == ProposalStatus.Submitted || p.Status == ProposalStatus.Active || p.Status == ProposalStatus.Offer)
                            && p.JobPostId == dto.JobPostId, cancellationToken);

            if (exists)
            {
                return new Result<ProposalReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ProposalAlreadySubmitted,
                    Message = "You already have a submitted or active proposal/offer for this job post."
                };
            }

            // 5. Verify job post exists
            var job = await _context.JobPosts.FindAsync(new object[] { dto.JobPostId }, cancellationToken);
            if (job == null)
            {
                return new Result<ProposalReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.JobNotFound,
                    Message = $"Job post with ID {dto.JobPostId} not found."
                };
            }

            // Validate job-type specific requirements - Enforce single-payment only
            if (dto.Terms != null && dto.Terms.Any())
            {
                return new Result<ProposalReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = "Validation failed",
                    Errors = new List<string> { "Milestone-based proposals are not supported right now. Proposals must be single-payment (do not provide milestone terms)." }
                };
            }

            // 6. Calculate HORR Fee
            decimal horrFee = Math.Round(dto.BidRate * 0.10m, 2);

            // 7. Create entity
            var proposal = new Proposal
            {
                JobPostId = dto.JobPostId,
                FreelancerId = request.FreelancerId,
                SubmitAsType = dto.SubmitAsType,
                BidRate = dto.BidRate,
                BidCurrency = dto.BidCurrency ?? "USD",
                HORRFee = horrFee,
                CoverLetter = dto.CoverLetter,
                MaxRevisions = dto.MaxRevisions,
                DurationDays = dto.DurationDays,
                Status = ProposalStatus.Submitted,
                CreatedAt = DateTime.UtcNow,
                Terms = new List<ProposalTerm>
                {
                    new ProposalTerm
                    {
                        MilestoneTitle = "Single Payment",
                        Amount = dto.BidRate,
                        DueDate = DateTime.UtcNow.AddDays(14)
                    }
                }
            };

            _context.Proposals.Add(proposal);

            // Automatically track the apply interaction
            _context.Interactions.Add(new Entities.Users.Interactions
            {
                UserId = request.FreelancerId,
                TargetId = dto.JobPostId,
                TargetType = "job",
                Action = Entities.Enums.InteractionTypes.Apply,
                CreatedAt = DateTime.UtcNow
            });

            // 8. Auto-accept any pending invitation for this (freelancer, job) pair.
            // When a freelancer submits a proposal, it implicitly means they are accepting
            // the invitation — no separate accept endpoint is needed.
            var pendingInvitation = await _context.JobInvitations
                .FirstOrDefaultAsync(
                    i => i.FreelancerId == request.FreelancerId
                      && i.JobPostId == dto.JobPostId
                      && i.Status == InvitationStatus.Pending,
                    cancellationToken);

            if (pendingInvitation != null)
            {
                pendingInvitation.Status = InvitationStatus.Accepted;
                pendingInvitation.RespondedAt = DateTime.UtcNow;
                // ProposalId will be set after SaveChangesAsync when EF populates the generated ID
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Link the generated proposal ID to the invitation now that EF has assigned it
            if (pendingInvitation != null)
            {
                pendingInvitation.ProposalId = proposal.Id;
                await _context.SaveChangesAsync(cancellationToken);
            }


            return new Result<ProposalReadDTO>
            {
                Succeeded = true,
                Data = new ProposalReadDTO
                {
                    Id = proposal.Id,
                    JobPostId = proposal.JobPostId,
                    FreelancerName = user.FullName,
                    JobPostTitle = job.Title,
                    FreelancerId = proposal.FreelancerId,
                    SubmitAsType = proposal.SubmitAsType,
                    BidRate = proposal.BidRate,
                    BidCurrency = proposal.BidCurrency,
                    HORRFee = proposal.HORRFee,
                    CoverLetter = proposal.CoverLetter,
                    MaxRevisions = proposal.MaxRevisions,
                    DurationDays = proposal.DurationDays,
                    Status = proposal.Status,
                    CreatedAt = proposal.CreatedAt,
                    Terms = proposal.Terms.Select(t => new ProposalTermReadDTO
                    {
                        Id = t.Id,
                        MilestoneTitle = t.MilestoneTitle,
                        Amount = t.Amount,
                        DueDate = t.DueDate
                    }).ToList()
                }
            };
        }

        private List<string> Validate(ProposalCreateDTO dto)
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

            return errors;
        }
    }
}
