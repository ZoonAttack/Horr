using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Proposal;
using Entities.Enums;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Proposals
{
    public class GetMyProposalsQueryHandler : IRequestHandler<GetMyProposalsQuery, Result<MyProposalsResponseDto>>
    {
        private readonly AppDbContext _context;

        public GetMyProposalsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<MyProposalsResponseDto>> Handle(GetMyProposalsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<MyProposalsResponseDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Freelancer account not found or is deleted."
                };
            }

            var proposals = await _context.Proposals
                .Include(p => p.JobPost)
                .Include(p => p.Terms)
                .Where(p => p.FreelancerId == request.FreelancerId)
                .ToListAsync(cancellationToken);

            var proposalIds = proposals.Select(p => p.Id).ToList();
            var proposalContracts = await _context.Contracts
                .Where(c => c.ProposalId != null && proposalIds.Contains(c.ProposalId.Value))
                .ToDictionaryAsync(c => c.ProposalId!.Value, c => c.Id, cancellationToken);

            var response = new MyProposalsResponseDto();

            foreach (var p in proposals)
            {
                var dto = MapToDto(p);
                if (proposalContracts.TryGetValue(p.Id, out int contractId))
                {
                    dto.ContractId = contractId;
                }

                switch (p.Status)
                {
                    case ProposalStatus.Active:
                        response.Active.Add(dto);
                        break;
                    case ProposalStatus.Submitted:
                        response.Submitted.Add(dto);
                        break;
                    case ProposalStatus.Offer:
                        response.Offers.Add(dto);
                        break;
                }
            }

            return new Result<MyProposalsResponseDto> { Succeeded = true, Data = response };
        }

        private ProposalReadDTO MapToDto(Entities.Project.Proposal p)
        {
            return new ProposalReadDTO
            {
                Id = p.Id,
                JobPostId = p.JobPostId,
                JobPostTitle = p.JobPost.Title,
                FreelancerId = p.FreelancerId,
                SubmitAsType = p.SubmitAsType,
                BidRate = p.BidRate,
                HORRFee = p.HORRFee,
                CoverLetter = p.CoverLetter,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                Terms = p.Terms.Select(t => new ProposalTermReadDTO
                {
                    Id = t.Id,
                    MilestoneTitle = t.MilestoneTitle,
                    Amount = t.Amount,
                    DueDate = t.DueDate
                }).ToList()
            };
        }
    }
}
