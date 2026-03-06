using Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Mappings;
using Entities.Enums;

namespace ServiceImplementation.Implementations.JobManagement
{
    public class SearchJobsQueryHandler : IRequestHandler<SearchJobsQuery, SearchJobsQueryResponse>
    {
        private readonly AppDbContext _context;

        public SearchJobsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SearchJobsQueryResponse> Handle(SearchJobsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.JobPosts
                .Include(j => j.Client)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.SavedByFreelancers) // Needed for IsSaved
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(j => j.Title.Contains(request.Keyword) || j.Description.Contains(request.Keyword));
            }

            if (request.JobType.HasValue)
            {
                query = query.Where(j => j.JobType == request.JobType.Value);
            }

            // Sorting
            query = request.SortBy switch
            {
                JobSortEnum.Oldest => query.OrderBy(j => j.PostedAt),
                JobSortEnum.Budget => query.OrderByDescending(j => j.BudgetMax),
                _ => query.OrderByDescending(j => j.PostedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new SearchJobsQueryResponse
            {
                Items = items.Select(j => j.ToSummaryDto(request.CurrentUserId)).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
