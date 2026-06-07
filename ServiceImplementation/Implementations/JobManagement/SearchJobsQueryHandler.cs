using Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Mappings;
using Entities.Enums;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.JobManagement
{
    public class SearchJobsQueryHandler : IRequestHandler<SearchJobsQuery, Result<SearchJobsQueryResponse>>
    {
        private readonly AppDbContext _context;

        public SearchJobsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<SearchJobsQueryResponse>> Handle(SearchJobsQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.CurrentUserId))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, cancellationToken);
                if (user == null || user.IsDeleted)
                {
                    return new Result<SearchJobsQueryResponse>
                    {
                        Succeeded = false,
                        ErrorCode = ErrorCodes.AccountDeleted,
                        Message = "Account not found or is deleted."
                    };
                }
            }

            var query = _context.JobPosts
                .Include(j => j.Client)
                .Include(j => j.Category)
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
                JobSortEnum.Budget => query.OrderByDescending(j => j.Budget),
                _ => query.OrderByDescending(j => j.PostedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var response = new SearchJobsQueryResponse
            {
                Items = items.Select(j => j.ToSummaryDto(request.CurrentUserId)).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return new Result<SearchJobsQueryResponse> { Succeeded = true, Data = response };
        }
    }
}
