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
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ServiceImplementation.Implementations.Proposals
{
    public class UpdateProposalCommandHandler : IRequestHandler<UpdateProposalCommand, Result<ProposalReadDTO>>
    {
        private readonly AppDbContext _context;

        public UpdateProposalCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ProposalReadDTO>> Handle(UpdateProposalCommand request, CancellationToken cancellationToken)
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

            // 2. Verify user is a registered freelancer
            var freelancerExists = await _context.Freelancers
                .AnyAsync(f => f.UserId == request.FreelancerId, cancellationToken);

            if (!freelancerExists)
            {
                throw new NotFoundException("You are not registered as a freelancer. Please complete your freelancer profile first.");
            }

            // 3. Find proposal
            var proposal = await _context.Proposals
                .Include(p => p.Terms)
                .Include(p => p.JobPost)
                .FirstOrDefaultAsync(p => p.Id == request.ProposalId && p.FreelancerId == request.FreelancerId, cancellationToken);

            if (proposal == null)
            {
                throw new NotFoundException($"Proposal with ID {request.ProposalId} not found or you are not authorized to update it.");
            }

            // 4. Verify proposal is in Submitted state
            if (proposal.Status != ProposalStatus.Submitted)
            {
                throw new InvalidStateException("Proposals can only be updated when they are in the Submitted state.");
            }

            // 5. Domain Validation
            Validate(dto);

            // 6. Update proposal rates &cover letter
            proposal.BidRate = dto.BidRate;
            proposal.CoverLetter = dto.CoverLetter;
            proposal.HORRFee = Math.Round(dto.BidRate * 0.10m, 2);

            // 7. Keep the single payment term in sync
            var singleTerm = proposal.Terms.FirstOrDefault();
            if (singleTerm != null)
            {
                singleTerm.Amount = dto.BidRate;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new Result<ProposalReadDTO>
            {
                Succeeded = true,
                Data = new ProposalReadDTO
                {
                    Id = proposal.Id,
                    JobPostId = proposal.JobPostId,
                    FreelancerName = user.FullName,
                    JobPostTitle = proposal.JobPost?.Title ?? string.Empty,
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
                }
            };
        }

        private void Validate(ProposalUpdateDTO dto)
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
