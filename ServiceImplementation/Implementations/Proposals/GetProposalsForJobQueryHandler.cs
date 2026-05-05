using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Proposal;
using Services;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Proposals
{
    public class GetProposalsForJobQueryHandler : IRequestHandler<GetProposalsForJobQuery, PagedResult<ProposalSummaryForClientDto>>
    {
        private readonly AppDbContext _context;

        public GetProposalsForJobQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProposalSummaryForClientDto>> Handle(GetProposalsForJobQuery request, CancellationToken cancellationToken)
        {
            var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);
            if (job == null)
            {
                throw new NotFoundException($"Job with ID {request.JobId} not found.");
            }

            if (job.ClientId != request.ClientId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view proposals for this job.");
            }

            var query = _context.Proposals
                .Include(p => p.Freelancer)
                .ThenInclude(f => f.User)
                .Where(p => p.JobPostId == request.JobId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProposalSummaryForClientDto
                {
                    Id = p.Id,
                    FreelancerId = p.FreelancerId,
                    FreelancerName = p.Freelancer.User.FullName,
                    BidRate = p.BidRate,
                    CoverLetter = p.CoverLetter,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ProposalSummaryForClientDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
