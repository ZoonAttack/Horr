using Entities;
using Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public class SearchFreelancersQueryHandler : IRequestHandler<SearchFreelancersQuery, Result<PagedResult<FreelancerSearchResultDTO>>>
    {
        private readonly AppDbContext _context;

        public SearchFreelancersQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<FreelancerSearchResultDTO>>> Handle(SearchFreelancersQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                var requestingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId, cancellationToken);
                if (requestingUser == null || requestingUser.IsDeleted)
                {
                    return new Result<PagedResult<FreelancerSearchResultDTO>>
                    {
                        Succeeded = false,
                        ErrorCode = ErrorCodes.AccountDeleted,
                        Message = "Account not found or is deleted."
                    };
                }
            }
            var query = _context.Users
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.FreelancerSkills)
                        .ThenInclude(fs => fs.Skill)
                .Where(u => u.Role == Entities.Enums.UserRole.Freelancer && !u.IsDeleted);

            // Basic search query on name or bio
            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                string searchLower = request.SearchQuery.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(searchLower) ||
                    (u.Bio != null && u.Bio.ToLower().Contains(searchLower)));
            }

            // Filters
            if (request.MinHourlyRate.HasValue)
            {
                query = query.Where(u => u.Freelancer.HourlyRate >= request.MinHourlyRate.Value);
            }

            if (request.MaxHourlyRate.HasValue)
            {
                query = query.Where(u => u.Freelancer.HourlyRate <= request.MaxHourlyRate.Value);
            }

            if (request.MinYearsExperience.HasValue)
            {
                query = query.Where(u => u.Freelancer.YearsOfExperience >= request.MinYearsExperience.Value);
            }

            if (request.MinTrustScore.HasValue)
            {
                query = query.Where(u => u.TrustScore >= request.MinTrustScore.Value);
            }

            if (request.IsVerified.HasValue)
            {
                query = query.Where(u => u.IsVerified == request.IsVerified.Value);
            }

            if (request.SkillIds != null && request.SkillIds.Any())
            {
                query = query.Where(u => u.Freelancer.FreelancerSkills.Any(fs => request.SkillIds.Contains(fs.SkillId)));
            }

            // Sorting
            query = request.SortBy switch
            {
                "HourlyRate" => request.SortDescending
                    ? query.OrderByDescending(u => u.Freelancer.HourlyRate)
                    : query.OrderBy(u => u.Freelancer.HourlyRate),
                "YearsExperience" => request.SortDescending
                    ? query.OrderByDescending(u => u.Freelancer.YearsOfExperience)
                    : query.OrderBy(u => u.Freelancer.YearsOfExperience),
                "TrustScore" => request.SortDescending
                    ? query.OrderByDescending(u => u.TrustScore)
                    : query.OrderBy(u => u.TrustScore),
                _ => query.OrderByDescending(u => u.TrustScore)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var savedFreelancerIds = new HashSet<string>();
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                savedFreelancerIds = (await _context.SavedFreelancers
                    .Where(sf => sf.ClientId == request.ClientId)
                    .Select(sf => sf.FreelancerId)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();
            }

            var freelancerIds = items.Select(u => u.Id).ToList();

            var reviewsData = await _context.ContractReviews
                .Where(r => freelancerIds.Contains(r.Contract.FreelancerId) && r.ReviewerId == r.Contract.ClientId)
                .GroupBy(r => r.Contract.FreelancerId)
                .Select(g => new {
                    FreelancerId = g.Key,
                    AverageRating = g.Average(r => r.Rating),
                    TotalReviews = g.Count()
                })
                .ToDictionaryAsync(x => x.FreelancerId, x => x, cancellationToken);

            var contractsData = await _context.Contracts
                .Where(c => freelancerIds.Contains(c.FreelancerId) && !c.IsDeleted)
                .GroupBy(c => c.FreelancerId)
                .Select(g => new {
                    FreelancerId = g.Key,
                    CompletedCount = g.Count(c => c.Status == Entities.Enums.ContractStatus.Completed),
                    ClosedCount = g.Count(c => c.Status == Entities.Enums.ContractStatus.Closed),
                    TerminatedCount = g.Count(c => c.Status == Entities.Enums.ContractStatus.Terminated)
                })
                .ToDictionaryAsync(x => x.FreelancerId, x => x, cancellationToken);

            var dtos = items.Select(u =>
            {
                double avgRating = 0.0;
                int totalReviews = 0;
                if (reviewsData.TryGetValue(u.Id, out var reviewStats))
                {
                    avgRating = Math.Round(reviewStats.AverageRating, 1);
                    totalReviews = reviewStats.TotalReviews;
                }

                int jobSuccessPercentage = 100;
                if (contractsData.TryGetValue(u.Id, out var contractStats))
                {
                    var totalEnded = contractStats.CompletedCount + contractStats.ClosedCount + contractStats.TerminatedCount;
                    if (totalEnded > 0)
                    {
                        jobSuccessPercentage = (int)Math.Round((double)contractStats.CompletedCount / totalEnded * 100);
                    }
                }

                bool isSaved = savedFreelancerIds.Contains(u.Id);

                return u.Freelancer_To_FreelancerSearchResult(isSaved, avgRating, totalReviews, jobSuccessPercentage);
            }).ToList();

            return new Result<PagedResult<FreelancerSearchResultDTO>>
            {
                Succeeded = true,
                Data = new PagedResult<FreelancerSearchResultDTO>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                }
            };
        }
    }

    public class GetSavedFreelancersQueryHandler : IRequestHandler<GetSavedFreelancersQuery, Result<PagedResult<FreelancerSearchResultDTO>>>
    {
        private readonly AppDbContext _context;

        public GetSavedFreelancersQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<FreelancerSearchResultDTO>>> Handle(GetSavedFreelancersQuery request, CancellationToken cancellationToken)
        {
            var requestingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId, cancellationToken);
            if (requestingUser == null || requestingUser.IsDeleted)
            {
                return new Result<PagedResult<FreelancerSearchResultDTO>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }
            var query = _context.SavedFreelancers
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.User)
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.FreelancerSkills)
                        .ThenInclude(fs => fs.Skill)
                .Where(sf => sf.ClientId == request.ClientId)
                .OrderByDescending(sf => sf.SavedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var savedItems = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var freelancerIds = savedItems
                .Where(sf => sf.Freelancer != null && sf.Freelancer.User != null)
                .Select(sf => sf.Freelancer.User.Id)
                .ToList();

            var reviewsData = await _context.ContractReviews
                .Where(r => freelancerIds.Contains(r.Contract.FreelancerId) && r.ReviewerId == r.Contract.ClientId)
                .GroupBy(r => r.Contract.FreelancerId)
                .Select(g => new {
                    FreelancerId = g.Key,
                    AverageRating = g.Average(r => r.Rating),
                    TotalReviews = g.Count()
                })
                .ToDictionaryAsync(x => x.FreelancerId, x => x, cancellationToken);

            var contractsData = await _context.Contracts
                .Where(c => freelancerIds.Contains(c.FreelancerId) && !c.IsDeleted)
                .GroupBy(c => c.FreelancerId)
                .Select(g => new {
                    FreelancerId = g.Key,
                    CompletedCount = g.Count(c => c.Status == Entities.Enums.ContractStatus.Completed),
                    ClosedCount = g.Count(c => c.Status == Entities.Enums.ContractStatus.Closed),
                    TerminatedCount = g.Count(c => c.Status == Entities.Enums.ContractStatus.Terminated)
                })
                .ToDictionaryAsync(x => x.FreelancerId, x => x, cancellationToken);

            // Note: f.User is the Entities.Users.User object that contains Freelancer_To_FreelancerSearchResult()
            var dtos = savedItems
                .Where(sf => sf.Freelancer != null && sf.Freelancer.User != null)
                .Select(sf =>
                {
                    var u = sf.Freelancer.User;
                    double avgRating = 0.0;
                    int totalReviews = 0;
                    if (reviewsData.TryGetValue(u.Id, out var reviewStats))
                    {
                        avgRating = Math.Round(reviewStats.AverageRating, 1);
                        totalReviews = reviewStats.TotalReviews;
                    }

                    int jobSuccessPercentage = 100;
                    if (contractsData.TryGetValue(u.Id, out var contractStats))
                    {
                        var totalEnded = contractStats.CompletedCount + contractStats.ClosedCount + contractStats.TerminatedCount;
                        if (totalEnded > 0)
                        {
                            jobSuccessPercentage = (int)Math.Round((double)contractStats.CompletedCount / totalEnded * 100);
                        }
                    }

                    return u.Freelancer_To_FreelancerSearchResult(isSaved: true, averageRating: avgRating, totalReviews: totalReviews, jobSuccessPercentage: jobSuccessPercentage);
                })
                .ToList();

            return new Result<PagedResult<FreelancerSearchResultDTO>>
            {
                Succeeded = true,
                Data = new PagedResult<FreelancerSearchResultDTO>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                }
            };
        }
    }
}
