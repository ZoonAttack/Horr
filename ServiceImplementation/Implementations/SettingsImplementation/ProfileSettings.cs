using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities; // Using the AppDbContext directly for simplicity if a specific UserRepository doesn't encompass all updates
using ServiceContracts.DTOs.Settings;
using ServiceContracts.Settings;
using Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace ServiceImplementation.Implementations.Settings
{
    public class ProfileSettings : IProfileSettings
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProfileSettings(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public Task<bool> UpdateFullNameAsync(string userId, string newName)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.IsDeleted) return Task.FromResult(false);
            
            user.FullName = newName;
            _userManager.UpdateAsync(user).Wait();

            return Task.FromResult(true);
        }

        public Task<bool> UpdateEmailAsync(string userId, string newEmail)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.IsDeleted) return Task.FromResult(false);

            _userManager.SetEmailAsync(user, newEmail).Wait();
            //The above line should be changed later(I kept it like this for now to avoid breaking changes
            //In a real implementation. a token should be gnerated and sent to the new email
            //The user then clicks on the link sent to the new email to confirm the change
            //Then using the ChangeEmailAsync method to update the email after confirmation

            return Task.FromResult(true);
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

            if (dto.Bio != null)
                user.Bio = dto.Bio;

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
