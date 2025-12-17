using ServiceContracts.DTOs.User.Freelancer;
using Entities.Users;
using Services;
using ServiceImplementation.Authentication.Helpers;
using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Entities.Users.FreelancerHelpers;

namespace ServiceImplementation.Authentication.User
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

        #region Reconcile Helpers

        /// <summary>
        /// Synchronizes the existing Languages collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileLanguages(Freelancer freelancer, ICollection<LanguageUpdateDto> incomingDtos, string freelancerId)
        {
            var existing = freelancer.Languages;
            var incomingIds = incomingDtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id.Value).ToList();

            // 1. DELETE: Identify existing records whose IDs are NOT present in the incoming DTO list.
            var itemsToDelete = existing.Where(e => !incomingIds.Contains(e.Id)).ToList();
            if (itemsToDelete.Any())
            {
                _db.FreelancerLanguages.RemoveRange(itemsToDelete);
            }

            // 2. ADD/UPDATE:
            foreach (var dto in incomingDtos)
            {
                if (dto.Id.HasValue)
                {
                    // UPDATE: Item exists, find it and modify
                    var entity = existing.First(e => e.Id == dto.Id.Value);
                    entity.Name = dto.Name;
                    entity.Level = dto.Level;
                }
                else
                {
                    // ADD: Item is new (Id is null), create entity and add
                    var newEntity = dto.ToEntity(freelancerId);
                    existing.Add(newEntity);
                }
            }
        }

        /// <summary>
        /// Synchronizes the existing Education collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileEducation(Freelancer freelancer, ICollection<EducationUpdateDto> incomingDtos, string freelancerId)
        {
            var existing = freelancer.Education;
            var incomingIds = incomingDtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id.Value).ToList();

            var itemsToDelete = existing.Where(e => !incomingIds.Contains(e.Id)).ToList();
            if (itemsToDelete.Any())
            {
                _db.FreelancerEducation.RemoveRange(itemsToDelete);
            }

            foreach (var dto in incomingDtos)
            {
                if (dto.Id.HasValue)
                {
                    var entity = existing.First(e => e.Id == dto.Id.Value);
                    entity.School = dto.School;
                    entity.DateStart = dto.DateStart;
                    entity.DateEnd = dto.DateEnd;
                    entity.Degree = dto.Degree;
                    entity.FieldOfStudy = dto.FieldOfStudy;
                }
                else
                {
                    var newEntity = dto.ToEntity(freelancerId);
                    existing.Add(newEntity);
                }
            }
        }

        /// <summary>
        /// Synchronizes the existing ExperienceDetails collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileExperienceDetails(Freelancer freelancer, ICollection<ExperienceDetailUpdateDto> incomingDtos, string freelancerId)
        {
            var existing = freelancer.ExperienceDetails;
            var incomingIds = incomingDtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id.Value).ToList();

            var itemsToDelete = existing.Where(e => !incomingIds.Contains(e.Id)).ToList();
            if (itemsToDelete.Any())
            {
                _db.FreelancerExperienceDetails.RemoveRange(itemsToDelete);
            }

            foreach (var dto in incomingDtos)
            {
                if (dto.Id.HasValue)
                {
                    var entity = existing.First(e => e.Id == dto.Id.Value);
                    entity.Subject = dto.Subject;
                    entity.Description = dto.Description;
                }
                else
                {
                    var newEntity = dto.ToEntity(freelancerId);
                    existing.Add(newEntity);
                }
            }
        }

        /// <summary>
        /// Synchronizes the existing EmploymentHistory collection with the incoming DTOs
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="incomingDtos"></param>
        /// <param name="freelancerId"></param>
        private void ReconcileEmployment(Freelancer freelancer, ICollection<ServiceContracts.DTOs.User.Freelancer.EmploymentUpdateDto> incomingDtos, string freelancerId)
        {
            var existing = freelancer.EmploymentHistory;
            var incomingIds = incomingDtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id.Value).ToList();

            var itemsToDelete = existing.Where(e => !incomingIds.Contains(e.Id)).ToList();
            if (itemsToDelete.Any())
            {
                _db.FreelancerEmploymentHistory.RemoveRange(itemsToDelete);
            }

            foreach (var dto in incomingDtos)
            {
                if (dto.Id.HasValue)
                {
                    var entity = existing.First(e => e.Id == dto.Id.Value);
                    entity.Company = dto.Company;
                    entity.City = dto.City;
                    entity.Country = dto.Country;
                    entity.Title = dto.Title;
                    entity.CurrentlyWorkThere = dto.CurrentlyWorkThere;
                    entity.FromDate = dto.FromDate;
                    entity.ToDate = dto.ToDate;
                }
                else
                {
                    FreelancerEmployment newEntity = new FreelancerEmployment
                    {
                        Id = dto.Id ?? 0,
                        FreelancerId = freelancerId,
                        Company = dto.Company,
                        City = dto.City,
                        Country = dto.Country,
                        Title = dto.Title,
                        CurrentlyWorkThere = dto.CurrentlyWorkThere,
                        FromDate = dto.FromDate,
                        ToDate = dto.ToDate
                    };

                    existing.Add(newEntity);
                }
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

        public async Task<bool> UpdateFreelancerAsync(FreelancerUpdateDTO freelancerUpdateDTO)
        {
            if (freelancerUpdateDTO == null)
            {
                throw new ArgumentNullException(nameof(freelancerUpdateDTO));
            }

            // getting the authenticated user id
            string currentId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Unable to retrieve the authenticated user ID.");

            // 1. fetch the existing user
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

                .FirstOrDefaultAsync(u => u.Id == currentId && u.Role == Entities.Enums.UserRole.Freelancer && !u.IsDeleted);

            if (user == null || user.Freelancer == null)
            {
                return false;
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