using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using Entities.Users;
using Services;
using ServiceImplementation.Authentication.Helpers;
using Entities;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Entities.Users.FreelancerHelpers;
using Services.Freelancer;

namespace ServiceImplementation.Implementations.FreelancerImplementation
{
    public class FreelancerService : IFreelancerService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public FreelancerService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Helper Methods

        /// <summary>
        /// helper method to apply sorting
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortDescending"></param>
        /// <returns></returns>
        private IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, bool sortDescending)
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

        #region Reconcile Helpers

        /// <summary>
        /// Synchronizes the existing Languages collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileLanguages(Freelancer freelancer, ICollection<LanguageUpdateDto>? incomingDtos, string freelancerId)
        {
            if (incomingDtos == null) return;

            // 1. Load existing records directly from the DB context for this freelancer
            var existing = _db.FreelancerLanguages
                .Where(l => l.FreelancerId == freelancerId)
                .ToList();

            // 2. Remove all existing
            _db.FreelancerLanguages.RemoveRange(existing);

            // 3. Add new ones from DTO
            foreach (var dto in incomingDtos)
            {
                _db.FreelancerLanguages.Add(new FreelancerLanguage
                {
                    FreelancerId = freelancerId,
                    Name = dto.Name,
                    Level = dto.Level
                });
            }
        }

        /// <summary>
        /// Synchronizes the existing Education collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileEducation(Freelancer freelancer, ICollection<EducationUpdateDto>? incomingDtos, string freelancerId)
        {
            if (incomingDtos == null) return;

            var existing = _db.FreelancerEducation
                .Where(e => e.FreelancerId == freelancerId)
                .ToList();

            _db.FreelancerEducation.RemoveRange(existing);

            foreach (var dto in incomingDtos)
            {
                _db.FreelancerEducation.Add(new FreelancerEducation
                {
                    FreelancerId = freelancerId,
                    School = dto.School ?? string.Empty,
                    Degree = dto.Degree ?? string.Empty,
                    FieldOfStudy = dto.FieldOfStudy ?? string.Empty,
                    DateStart = dto.DateStart ?? DateTime.UtcNow,
                    DateEnd = dto.DateEnd
                });
            }
        }

        /// <summary>
        /// Synchronizes the existing ExperienceDetails collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileExperienceDetails(Freelancer freelancer, ICollection<ExperienceDetailUpdateDto>? incomingDtos, string freelancerId)
        {
            if (incomingDtos == null) return;

            var existing = _db.FreelancerExperienceDetails
                .Where(e => e.FreelancerId == freelancerId)
                .ToList();

            _db.FreelancerExperienceDetails.RemoveRange(existing);

            foreach (var dto in incomingDtos)
            {
                _db.FreelancerExperienceDetails.Add(new FreelancerExperienceDetail
                {
                    FreelancerId = freelancerId,
                    Subject = dto.Subject,
                    Description = dto.Description
                });
            }
        }

        /// <summary>
        /// Synchronizes the existing EmploymentHistory collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileEmployment(Freelancer freelancer, ICollection<EmploymentUpdateDto>? incomingDtos, string freelancerId)
        {
            if (incomingDtos == null) return;

            var existing = _db.FreelancerEmploymentHistory
                .Where(e => e.FreelancerId == freelancerId)
                .ToList();

            _db.FreelancerEmploymentHistory.RemoveRange(existing);

            foreach (var dto in incomingDtos)
            {
                _db.FreelancerEmploymentHistory.Add(new FreelancerEmployment
                {
                    FreelancerId = freelancerId,
                    Company = dto.Company ?? string.Empty,
                    City = dto.City ?? string.Empty,
                    Country = dto.Country ?? string.Empty,
                    Title = dto.Title ?? string.Empty,
                    CurrentlyWorkThere = dto.CurrentlyWorkThere ?? false,
                    FromDate = dto.FromDate ?? DateTime.UtcNow,
                    ToDate = dto.ToDate
                });
            }
        }

        #endregion

        #endregion

        public async Task<FreelancerReadDTO> CreateFreelancerAsync(FreelancerCreateDTO freelancerCreationDTO)
        {
            if (freelancerCreationDTO == null)
            {
                throw new ArgumentNullException(nameof(freelancerCreationDTO));
            }

            ValidationHelper.ModelValidation(freelancerCreationDTO);

            User user = freelancerCreationDTO.FreelancerCreate_To_User();

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

        public async Task<bool> DeleteFreelancerAsync(string freelancerId)
        {
            if (string.IsNullOrEmpty(freelancerId))
            {
                throw new ArgumentException("Freelancer ID cannot be empty.", nameof(freelancerId));
            }

            // fetch the User entity
            var idString = freelancerId;
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
                await _db.ServiceCatalogItems
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
        

        public async Task<Services.PagedResult<FreelancerReadDTO>> GetAllFreelancersAsync(List<string>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            // 1. get all non-deleted freelancers
            IQueryable<User> query = _db.Users
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
            return new Services.PagedResult<FreelancerReadDTO>
            {
                Items = freelancersReadDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<FreelancerReadDTO?> GetFreelancerProfileByIdAsync(string freelancerId)
        {
            if (string.IsNullOrEmpty(freelancerId))
            {
                throw new ArgumentException("Freelancer ID cannot be empty.", nameof(freelancerId));        
            }
            var idString = freelancerId;

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

            if (freelancer == null || freelancer.User == null)
                return null;

            var clientReviews = await _db.ContractReviews
                .Where(cr => cr.Contract.FreelancerId == idString && cr.ReviewerId == cr.Contract.ClientId)
                .ToListAsync();
            var totalReviews = clientReviews.Count;
            var averageRating = totalReviews > 0 ? Math.Round(clientReviews.Average(r => r.Rating), 1) : 0.0;

            var contracts = await _db.Contracts
                .Where(c => c.FreelancerId == idString && !c.IsDeleted)
                .Select(c => c.Status)
                .ToListAsync();

            var completedCount = contracts.Count(s => s == ContractStatus.Completed);
            var closedCount = contracts.Count(s => s == ContractStatus.Closed);
            var terminatedCount = contracts.Count(s => s == ContractStatus.Terminated);
            var totalEnded = completedCount + closedCount + terminatedCount;
            var jobSuccessPercentage = totalEnded > 0 
                ? (int)Math.Round((double)completedCount / totalEnded * 100) 
                : 100;

            return freelancer.User.Freelancer_To_FreelancerRead(false, averageRating, totalReviews, jobSuccessPercentage);
        }

        public async Task<FreelancerPublicReadDTO?> GetFreelancerPublicProfileByIdAsync(string freelancerId)
        {
            if (string.IsNullOrEmpty(freelancerId))
            {
                throw new ArgumentException("Freelancer ID cannot be empty.", nameof(freelancerId));
            }
            var idString = freelancerId;

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

            if (freelancer == null || freelancer.User == null)
                return null;

            var clientReviews = await _db.ContractReviews
                .Where(cr => cr.Contract.FreelancerId == idString && cr.ReviewerId == cr.Contract.ClientId)
                .ToListAsync();
            var totalReviews = clientReviews.Count;
            var averageRating = totalReviews > 0 ? Math.Round(clientReviews.Average(r => r.Rating), 1) : 0.0;

            var contracts = await _db.Contracts
                .Where(c => c.FreelancerId == idString && !c.IsDeleted)
                .Select(c => c.Status)
                .ToListAsync();

            var completedCount = contracts.Count(s => s == ContractStatus.Completed);
            var closedCount = contracts.Count(s => s == ContractStatus.Closed);
            var terminatedCount = contracts.Count(s => s == ContractStatus.Terminated);
            var totalEnded = completedCount + closedCount + terminatedCount;
            var jobSuccessPercentage = totalEnded > 0 
                ? (int)Math.Round((double)completedCount / totalEnded * 100) 
                : 100;

            return freelancer.User.ToPublicReadDto(averageRating, totalReviews, jobSuccessPercentage);
        }

        public async Task<Services.PagedResult<FreelancerReadDTO>> SearchFreelancersAsync(string searchQuery, List<string>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            // 1. get all non-deleted freelancers
            IQueryable<User> query = _db.Users
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
                    u.Bio.ToLower().Contains(normalizedSearchQuery) ||
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
            return new Services.PagedResult<FreelancerReadDTO>
            {
                Items = freelancersReadDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> UpdateFreelancerAsync(string userId, FreelancerUpdateDTO freelancerUpdateDTO)
        {
            if (freelancerUpdateDTO == null)
            {
                throw new ArgumentNullException(nameof(freelancerUpdateDTO));
            }

            // 1. fetch the existing user with the strict role check (as it was)
            var user = await _db.Users
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
                .FirstOrDefaultAsync(u => u.Id == userId && u.Role == Entities.Enums.UserRole.Freelancer && !u.IsDeleted);

            // If not found with the strict role check, the user mentioned they are seeing users with Role 0 (Client) 
            // even if they have a freelancer record. Let's try to find them and fix the role if they have a profile.
            if (user == null)
            {
                user = await _db.Users
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
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

                if (user != null && user.Freelancer != null)
                {
                    // Fix the role mismatch discovered by the user
                    user.Role = Entities.Enums.UserRole.Freelancer;
                }
                else
                {
                    return false;
                }
            }

            if (user == null)
            {
                // Log or track why the user wasn't found
                return false;
            }

            if (user.Freelancer == null)
            {
                user.Freelancer = new Freelancer
                {
                    UserId = user.Id,
                    Title = freelancerUpdateDTO.Title ?? "Freelancer",
                    HourlyRate = freelancerUpdateDTO.HourlyRate ?? 0,
                    Availability = freelancerUpdateDTO.Availability ?? "More than 30 hrs/week",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Freelancers.Add(user.Freelancer);
            }

            // 2. update the base user and freelancer properties
            user.FreelancerUpdate_To_Freelancer(freelancerUpdateDTO);

            // 3. reconcile the collections
            ReconcileLanguages(user.Freelancer, freelancerUpdateDTO.Languages, user.Id);
            ReconcileEducation(user.Freelancer, freelancerUpdateDTO.Education, user.Id);
            ReconcileExperienceDetails(user.Freelancer, freelancerUpdateDTO.ExperienceDetails, user.Id);
            ReconcileEmployment(user.Freelancer, freelancerUpdateDTO.EmploymentHistory, user.Id);

            // 4. save changes and return
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
