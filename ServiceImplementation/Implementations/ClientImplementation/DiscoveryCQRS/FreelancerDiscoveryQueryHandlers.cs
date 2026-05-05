using Entities;
using Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public class SearchFreelancersQueryHandler : IRequestHandler<SearchFreelancersQuery, PagedResult<FreelancerReadDTO>>
    {
        private readonly AppDbContext _context;

        public SearchFreelancersQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<FreelancerReadDTO>> Handle(SearchFreelancersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.FreelancerSkills)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Languages)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Education)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.ExperienceDetails)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.EmploymentHistory)
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

            var dtos = items.Select(u => u.Freelancer_To_FreelancerRead()).ToList();

            return new PagedResult<FreelancerReadDTO>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }

    public class GetSavedFreelancersQueryHandler : IRequestHandler<GetSavedFreelancersQuery, PagedResult<FreelancerReadDTO>>
    {
        private readonly AppDbContext _context;

        public GetSavedFreelancersQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<FreelancerReadDTO>> Handle(GetSavedFreelancersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SavedFreelancers
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.User)
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.FreelancerSkills)
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.Languages)
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.Education)
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.ExperienceDetails)
                .Include(sf => sf.Freelancer)
                    .ThenInclude(f => f.EmploymentHistory)
                .Where(sf => sf.ClientId == request.ClientId)
                .OrderByDescending(sf => sf.SavedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var savedItems = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Note: f.User is the Entities.Users.User object that contains Freelancer_To_FreelancerRead()
            var dtos = savedItems
                .Where(sf => sf.Freelancer != null && sf.Freelancer.User != null)
                .Select(sf => sf.Freelancer.User.Freelancer_To_FreelancerRead())
                .ToList();

            return new PagedResult<FreelancerReadDTO>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
