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
using ServiceContracts.Currency;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public class SearchFreelancersQueryHandler : IRequestHandler<SearchFreelancersQuery, Result<PagedResult<FreelancerSearchResultDTO>>>
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyConverterService _currencyConverter;

        public SearchFreelancersQueryHandler(AppDbContext context, ICurrencyConverterService currencyConverter)
        {
            _context = context;
            _currencyConverter = currencyConverter;
        }

        public async Task<Result<PagedResult<FreelancerSearchResultDTO>>> Handle(SearchFreelancersQuery request, CancellationToken cancellationToken)
        {
            User? requestingUser = null;
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                requestingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId, cancellationToken);
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

            var targetCurrency = requestingUser?.PreferredCurrency ?? "USD";
            var dtos = new List<FreelancerSearchResultDTO>();
            
            foreach (var u in items)
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

                var dto = u.Freelancer_To_FreelancerSearchResult(isSaved, avgRating, totalReviews, jobSuccessPercentage);
                dto.OriginalCurrency = u.PreferredCurrency ?? "USD";
                
                if (dto.HourlyRate.HasValue)
                {
                    try
                    {
                        dto.ConvertedHourlyRate = await _currencyConverter.ConvertAsync(dto.HourlyRate.Value, dto.OriginalCurrency, targetCurrency);
                        dto.ConvertedCurrency = targetCurrency;
                    }
                    catch
                    {
                        dto.ConvertedHourlyRate = dto.HourlyRate;
                        dto.ConvertedCurrency = dto.OriginalCurrency;
                    }
                }
                
                dtos.Add(dto);
            }

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
        private readonly ICurrencyConverterService _currencyConverter;

        public GetSavedFreelancersQueryHandler(AppDbContext context, ICurrencyConverterService currencyConverter)
        {
            _context = context;
            _currencyConverter = currencyConverter;
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

            var targetCurrency = requestingUser.PreferredCurrency ?? "USD";
            var dtos = new List<FreelancerSearchResultDTO>();
            
            foreach (var sf in savedItems.Where(s => s.Freelancer != null && s.Freelancer.User != null))
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

                var dto = u.Freelancer_To_FreelancerSearchResult(isSaved: true, averageRating: avgRating, totalReviews: totalReviews, jobSuccessPercentage: jobSuccessPercentage);
                dto.OriginalCurrency = u.PreferredCurrency ?? "USD";

                if (dto.HourlyRate.HasValue)
                {
                    try
                    {
                        dto.ConvertedHourlyRate = await _currencyConverter.ConvertAsync(dto.HourlyRate.Value, dto.OriginalCurrency, targetCurrency);
                        dto.ConvertedCurrency = targetCurrency;
                    }
                    catch
                    {
                        dto.ConvertedHourlyRate = dto.HourlyRate;
                        dto.ConvertedCurrency = dto.OriginalCurrency;
                    }
                }
                
                dtos.Add(dto);
            }

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
