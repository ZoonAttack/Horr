using ServiceContracts.DTOs.User.Freelancer;
using Entities.Users;
using Services;
using ServiceImplementation.Authentication.Helpers;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace ServiceImplementation.Authentication.User
{
    public class FreelancerService : IFreelancerService
    {
        private readonly AppDbContext _db;
        public FreelancerService(AppDbContext db)
        {
            _db = db;
        }

        public FreelancerReadDTO freelancer_to_read(Freelancer freelancer)
        {
            return new FreelancerReadDTO
            {
                Id = freelancer.UserId.ToString(),
                Bio = freelancer.Bio,
                HourlyRate = freelancer.HourlyRate,
                Availability = freelancer.Availability,
                YearsOfExperience = freelancer.YearsOfExperience,
                PortfolioUrl = freelancer.PortfolioUrl,
                CreatedAt = freelancer.CreatedAt,
                UpdatedAt = freelancer.UpdatedAt
            };
        }

        public Task<FreelancerReadDTO> CreateFreelancerAsync(FreelancerCreateDTO freelancerCreationDTO)
        {
            if (freelancerCreationDTO == null)
            {
                throw new ArgumentNullException(nameof(freelancerCreationDTO));
            }

            ValidationHelper.ModelValidation(freelancerCreationDTO);

            Freelancer freelancer = freelancerCreationDTO.FreelancerCreate_To_User().Freelancer;

            freelancer.UserId = Guid.NewGuid().ToString();

            return Task.FromResult(freelancer_to_read(freelancer));
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
        

        public Task<PagedResult<FreelancerReadDTO>> GetAllFreelancersAsync(List<Guid>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
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
        .       FirstOrDefaultAsync(f =>
                    f.UserId == idString &&
                    f.User != null &&
                    !f.User.IsDeleted);

            if (freelancer == null)
                return null;

            return freelancer_to_read(freelancer);
        }

        public Task<FreelancerReadDTO?> GetFreelancerPublicProfileByIdAsync(Guid freelancerId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<FreelancerReadDTO>> SearchFreelancersAsync(string searchQuery, List<Guid>? skillIds = null, decimal? minHourlyRate = null, decimal? maxHourlyRate = null, int? minYearsExperience = null, decimal? minTrustScore = null, bool? isVerified = null, string? sortBy = "TrustScore", bool sortDescending = true, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateFreelancerAsync(FreelancerUpdateDTO freelancerUpdateDTO)
        {
            throw new NotImplementedException();
        }
    }
}
