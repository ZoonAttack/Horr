using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Proposal;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Proposals
{
    public class GetMyProposalsQueryHandler : IRequestHandler<GetMyProposalsQuery, MyProposalsResponseDto>
    {
        private readonly AppDbContext _context;

        public GetMyProposalsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MyProposalsResponseDto> Handle(GetMyProposalsQuery request, CancellationToken cancellationToken)
        {
            var proposals = await _context.Proposals
                .Include(p => p.JobPost)
                .Include(p => p.Terms)
                .Where(p => p.FreelancerId == request.FreelancerId)
                .ToListAsync(cancellationToken);

            var response = new MyProposalsResponseDto();

            foreach (var p in proposals)
            {
                var dto = MapToDto(p);

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

            return response;
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
