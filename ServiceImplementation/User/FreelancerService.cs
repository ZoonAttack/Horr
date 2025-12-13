using ServiceContracts.DTOs.User.Freelancer;
using Entities.Users;
using Services;
using ServiceImplementation.Authentication.Helpers;
using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ServiceImplementation.Authentication.User
{
    public class FreelancerService : IFreelancerService
    {
        private readonly AppDbContext _db;
        public FreelancerService(AppDbContext db)
        {
            _db = db;
        }
        
        /// <summary>
        /// helper method to apply sorting
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortDescending"></param>
        /// <returns></returns>
        private IQueryable<Entities.Users.User> ApplySorting(IQueryable<Entities.Users.User> query, string? sortBy, bool sortDescending)
        {
            // default sorting is by TrustScore descending
            var sortProperty = sortBy?.ToLowerInvariant() ?? "trustscore";

            switch (sortProperty)
            {
                case "fullname":
                    query = sortDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName);
                    break;
                case "hourlyrate":
                    query = sortDescending ? query.OrderByDescending(u => u.Freelancer!.HourlyRate) : query.OrderBy(u => u.Freelancer!.HourlyRate);
                    break;
                case "yearsofexperience":
                    query = sortDescending ? query.OrderByDescending(u => u.Freelancer!.YearsOfExperience) : query.OrderBy(u => u.Freelancer!.YearsOfExperience);
                    break;
                case "trustscore":
                default:
                    query = sortDescending ? query.OrderByDescending(u => u.TrustScore) : query.OrderBy(u => u.TrustScore);
                    break;
            }
            return query;
        }

        public async Task<FreelancerReadDTO> CreateFreelancerAsync(FreelancerCreateDTO freelancerCreationDTO)
        {
            if (freelancerCreationDTO == null)
            {
                throw new ArgumentNullException(nameof(freelancerCreationDTO));
            }

            ValidationHelper.ModelValidation(freelancerCreationDTO);

            Entities.Users.User user = freelancerCreationDTO.FreelancerCreate_To_User();

            user.Id = Guid.NewGuid().ToString();
            user.Role = Entities.Enums.UserRole.Freelancer;

            if (user.Freelancer == null)
            {
                throw new InvalidOperationException("Freelancer profile could not be created from DTO.");
            }

            string finalFreelancerId = user.Id;
            user.Freelancer.UserId = finalFreelancerId;

            // Iterate through the collections and fix the FreelancerId
            foreach (var lang in user.Freelancer.Languages) { lang.FreelancerId = finalFreelancerId; }
            foreach (var edu in user.Freelancer.Education) { edu.FreelancerId = finalFreelancerId; }
            foreach (var exp in user.Freelancer.ExperienceDetails) { exp.FreelancerId = finalFreelancerId; }
            foreach (var emp in user.Freelancer.EmploymentHistory) { emp.FreelancerId = finalFreelancerId; }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return await Task.FromResult(user.Freelancer_To_FreelancerRead());
        }

        public async Task<bool> DeleteFreelancerAsync(Guid freelancerId)
        {
            if (freelancerId == Guid.Empty)
            {
                throw new ArgumentException("Freelancer ID cannot be empty.", nameof(freelancerId));
            }

            // fetch the User entity
            var idString = freelancerId.ToString();
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == idString && u.Role == Entities.Enums.UserRole.Freelancer);

            if (user == null)
                return false;

            // if already deleted
            if (user.IsDeleted)
                return true;

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // soft delete the user
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                // soft delete their services
                await _db.Services
                    .Where(s => s.FreelancerId == idString && !s.IsDeleted)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(service => service.IsDeleted, true)
                        .SetProperty(service => service.DeletedAt, DateTime.UtcNow)
                        .SetProperty(service => service.IsActive, false));

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }
        

        public async Task<PagedResult<FreelancerReadDTO>> GetAllFreelancersAsync(List<string>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            // 1. get all non-deleted freelancers
            IQueryable<Entities.Users.User> query = _db.Users
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Languages)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Education)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.ExperienceDetails)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.EmploymentHistory)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.FreelancerSkills)
                        .ThenInclude(fs => fs.Skill)
                
                .Where(u => u.Role == Entities.Enums.UserRole.Freelancer && !u.IsDeleted && u.Freelancer != null);

            // 2. apply filters
            if (minHourlyRate.HasValue)
            {
                query = query.Where(u => u.Freelancer!.HourlyRate >= minHourlyRate.Value);
            }
            if (maxHourlyRate.HasValue)
            {
                query = query.Where(u => u.Freelancer!.HourlyRate <= maxHourlyRate.Value);
            }
            if (minYearsExperience.HasValue)
            {
                query = query.Where(u => u.Freelancer!.YearsOfExperience >= minYearsExperience.Value);
            }
            if (minTrustScore.HasValue)
            {
                query = query.Where(u => u.TrustScore >= minTrustScore.Value);
            }
            if (isVerified.HasValue)
            {
                query = query.Where(u => u.IsVerified == isVerified.Value);
            }
            if (skillIds != null && skillIds.Any())
            {
                query = query.Where(u => u.Freelancer!.FreelancerSkills!.Any(fs => skillIds.Contains(fs.SkillId)));
            }

            // 3. apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            // 4. apply pagination
            int totalCount = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;

            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            // 5. execute query and map to DTOs
            List<Freelancer> freelancers = await query
                .Select(u => u.Freelancer!)
                .ToListAsync();

            List<FreelancerReadDTO> freelancersReadDtos = await query
                .Select(f => f.Freelancer_To_FreelancerRead()).ToListAsync();

            // 6. return paged result
            return new PagedResult<FreelancerReadDTO>
            {
                Items = freelancersReadDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<FreelancerReadDTO?> GetFreelancerProfileByIdAsync(Guid freelancerId)
        {
            if (freelancerId == Guid.Empty)
            {
                throw new ArgumentException("Freelancer ID cannot be empty.", nameof(freelancerId));        
            }
            var idString = freelancerId.ToString();

            var freelancer = await _db.Freelancers
                .Include(f => f.User)
                .Include(f => f.Languages)
                .Include(f => f.Education)
                .Include(f => f.ExperienceDetails)
                .Include(f => f.EmploymentHistory)
        .       FirstOrDefaultAsync(f =>
                    f.UserId == idString &&
                    f.User != null &&
                    !f.User.IsDeleted);

            if (freelancer == null)
                return null;

            return freelancer.User.Freelancer_To_FreelancerRead();
        }

        public async Task<FreelancerPublicReadDTO?> GetFreelancerPublicProfileByIdAsync(Guid freelancerId)
        {
            if (freelancerId == Guid.Empty)
            {
                throw new ArgumentException("Freelancer ID cannot be empty.", nameof(freelancerId));
            }
            var idString = freelancerId.ToString();

            var freelancer = await _db.Freelancers
                .Include(f => f.User)
                .Include(f => f.Languages)
                .Include(f => f.Education)
                .Include(f => f.ExperienceDetails)
                .Include(f => f.EmploymentHistory)
                .FirstOrDefaultAsync(f =>
                    f.UserId == idString &&
                    f.User != null &&
                    !f.User.IsDeleted);

            if (freelancer == null)
                return null;

            if (freelancer.User == null)
                return null; 

            return freelancer.User.ToPublicReadDto();
        }

        public async Task<PagedResult<FreelancerReadDTO>> SearchFreelancersAsync(string searchQuery, List<string>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            // 1. get all non-deleted freelancers
            IQueryable<Entities.Users.User> query = _db.Users
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Languages)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Education)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.ExperienceDetails)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.EmploymentHistory)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.FreelancerSkills)
                        .ThenInclude(fs => fs.Skill)

                .Where(u => u.Role == Entities.Enums.UserRole.Freelancer && !u.IsDeleted && u.Freelancer != null);

            // 2. apply search query
            string normalizedSearchQuery = searchQuery?.Trim().ToLower() ?? string.Empty;

            if (!string.IsNullOrEmpty(normalizedSearchQuery))
            {
                // search in name, bio, and skills
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(normalizedSearchQuery) ||
                    u.Freelancer!.Bio.ToLower().Contains(normalizedSearchQuery) ||
                    u.Freelancer!.FreelancerSkills.Any(fs => fs.Skill.Name.ToLower().Contains(normalizedSearchQuery))
                );
            }

            // 3. apply filters
            if (minHourlyRate.HasValue)
            {
                query = query.Where(u => u.Freelancer!.HourlyRate >= minHourlyRate.Value);
            }
            if (maxHourlyRate.HasValue)
            {
                query = query.Where(u => u.Freelancer!.HourlyRate <= maxHourlyRate.Value);
            }
            if (minYearsExperience.HasValue)
            {
                query = query.Where(u => u.Freelancer!.YearsOfExperience >= minYearsExperience.Value);
            }
            if (minTrustScore.HasValue)
            {
                query = query.Where(u => u.TrustScore >= minTrustScore.Value);
            }
            if (isVerified.HasValue)
            {
                query = query.Where(u => u.IsVerified == isVerified.Value);
            }
            if (skillIds != null && skillIds.Any())
            {
                query = query.Where(u => u.Freelancer!.FreelancerSkills!.Any(fs => skillIds.Contains(fs.SkillId)));
            }

            // 3. apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            // 4. apply pagination
            int totalCount = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;

            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            // 5. execute query and map to DTOs
            List<Freelancer> freelancers = await query
                .Select(u => u.Freelancer!)
                .ToListAsync();

            List<FreelancerReadDTO> freelancersReadDtos = await query
                .Select(f => f.Freelancer_To_FreelancerRead()).ToListAsync();

            // 6. return paged result
            return new PagedResult<FreelancerReadDTO>
            {
                Items = freelancersReadDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<bool> UpdateFreelancerAsync(FreelancerUpdateDTO freelancerUpdateDTO)
        {
            throw new NotImplementedException();
        }
    }
}