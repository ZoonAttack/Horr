using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities; // Using the AppDbContext directly for simplicity if a specific UserRepository doesn't encompass all updates
using ServiceContracts.DTOs.Settings;
using ServiceContracts.Settings;
using Entities.Users;

namespace ServiceImplementation.Implementations.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly AppDbContext _context;

        public SettingsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateAccountAsync(Guid userId, AccountUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(userId.ToString());
            if (user == null || user.IsDeleted) return false;

            // Partial update: only update if value is provided
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email;
                user.NormalizedEmail = dto.Email.ToUpper();
                user.UserName = dto.Email;
                user.NormalizedUserName = dto.Email.ToUpper();
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLocationAsync(Guid userId, LocationUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(userId.ToString());
            if (user == null || user.IsDeleted) return false;

            // Partial update
            if (dto.Address != null) user.Address = dto.Address;
            if (dto.City != null) user.City = dto.City;
            if (dto.StateProvince != null) user.StateProvince = dto.StateProvince;
            if (dto.ZipCode != null) user.ZipCode = dto.ZipCode;
            if (dto.Country != null) user.Country = dto.Country;
            if (dto.TimeZone != null) user.TimeZone = dto.TimeZone;
            
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PrivacyResponseDto?> GetPrivacySettingsAsync(Guid userId)
        {
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId.ToString());
            if (freelancer == null) return null;

            // Requirement: HTML Prototype User ID Hash (e83b2bbd) -> Use first 8 chars of GUID for representation
            string hash = userId.ToString("N").Substring(0, 8);

            return new PrivacyResponseDto
            {
                UserIdHash = hash,
                Visibility = freelancer.VisibilityPreference,
                ExperienceLevel = freelancer.ExperienceLevel
            };
        }

        public async Task<bool> UpdatePrivacySettingsAsync(Guid userId, PrivacyUpdateDto dto)
        {
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId.ToString());
            if (freelancer == null) return false;

            if (dto.Visibility.HasValue)
                freelancer.VisibilityPreference = dto.Visibility.Value;

            if (dto.ExperienceLevel.HasValue)
                freelancer.ExperienceLevel = dto.ExperienceLevel.Value;

            _context.Freelancers.Update(freelancer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
